# HelpChat Lab Files

👉 The PowerPoint presentation, the Jupyter notebook, and code snippets are in the `_GOODIES INSIDE` folder.

> **_NOTE:_** This repo does NOT include the ONNX model because it's too big.
> - Download `model.onnx` from https://huggingface.co/protectai/deberta-v3-large-zeroshot-v1-onnx/tree/main.
> - Rename that file to `zero_shot_classifier.onnx` file, drop it in the AIModels folder (alongside `Labels.cs` and `vocab.txt`)
> - If required, set its **Build Action** to `Content` and **Copy to Output Directory** to `Copy if newer`

The Azure resource URLs/keys in App.xaml will need to be replaced, as they are no longer functional after the lab.
