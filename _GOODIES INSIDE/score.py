import onnxruntime
import json
import os
import numpy as np
from transformers import AutoTokenizer
import torch

def init():
    global session, tokenizer
    
    # Load the model
    model_dir = os.getenv('AZUREML_MODEL_DIR')
    if model_dir == None:
        model_dir = "./"
    model_path = os.path.join(model_dir, 'preconverted_deberta_large.onnx')
    session = onnxruntime.InferenceSession(model_path)
    
    # Load the tokenizer
    model_name = "MoritzLaurer/deberta-v3-large-zeroshot-v1"
    tokenizer = AutoTokenizer.from_pretrained(model_name)
    
def run(input_data):
    try:
        input_json = json.loads(input_data)
        text = input_json["text"]
        candidate_labels = input_json["labels"]
        
        # Preprocess and make predictions
        hypotheses = [f"This example is {label}." for label in candidate_labels]
        encoded = tokenizer([text] * len(candidate_labels), hypotheses, padding=True, truncation=True, return_tensors="np")
        
        logits = session.run(None, {
            "input_ids": encoded["input_ids"],
            "attention_mask": encoded["attention_mask"]
        })[0]
        
        # Method 6 (HF Pipeline Method): Extract entailment scores and normalize across labels
        entail_idx = 0  # For DeBERTa MNLI model: entailment is at index 0
        entail_scores = logits[:, entail_idx]  # Extract entailment scores for all labels
        
        # Normalize scores across labels using softmax
        normalized_scores = torch.softmax(torch.from_numpy(entail_scores), dim=0).numpy().tolist()
        
        # Create results and sort by score
        results = list(zip(candidate_labels, normalized_scores))
        results = sorted(results, key=lambda x: x[1], reverse=True)
        
        # Return as JSON
        return json.dumps(results)
    
    except Exception as e:
        error_message = str(e)
        return json.dumps({"error": error_message})