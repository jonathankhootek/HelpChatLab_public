using HelpChat.Utils;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using Microsoft.Windows.SemanticSearch;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace HelpChat.Services
{
    public class LocalClassifierService : IClassifierService
    {
        private readonly InferenceSession _inferenceSession;
        private string modelFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\AIModels\\zero_shot_classifier.onnx";
        private string tokenFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\AIModels\\vocab.txt";

        private BertTokenizer tokenizer;

        public LocalClassifierService()
        {
            //TODO Insert 9.1 below

            var sessionOptions = new SessionOptions();
            Dictionary<string, string> options = new()
                {
                    { "backend_path", "QnnHtp.dll" }
                };
            sessionOptions.AppendExecutionProvider("QNN", options);
            _inferenceSession = new InferenceSession(modelFile, sessionOptions);


            tokenizer = BertTokenizer.Create(tokenFile);

            //Insert above
        }

        public async Task<string> GetClassifiedLabel(string input, string[] labels)
        {
            return RunInference(input, labels).First().Label;
        }


        private List<(string Label, float Score)> RunInference(string input, string[] labels)
        {
            //TODO Insert 9.2 below (delete placeholder command)

            var scores = new List<float>();

            foreach (var label in labels)
            {
                var (inputIds, attentionMask) = Tokenize(input, label);

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length })),
                    NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length }))
                };

                using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _inferenceSession.Run(inputs);
                var logits = results.First().AsEnumerable<float>().ToArray();

                scores.Add(logits[0]);

            }

            // Softmax (scores --> relative probaibilties) across all entailment scores
            float max = scores.Max(); // for numerical stability
            float[] exp = scores.Select(s => MathF.Exp(s - max)).ToArray();
            float sum = exp.Sum();
            float[] probs = exp.Select(e => e / sum).ToArray();

            // Final labeled results
            var resultsWithLabels = labels.Zip(probs, (label, prob) => (label, prob))
                                          .OrderByDescending(x => x.prob)
                                          .ToList();

            // Sort and display
            var sorted = resultsWithLabels.OrderByDescending(r => r.prob);
            foreach (var result in sorted)
            {
                Log.Information($"🏷️ Local classifier says {result.label}: {result.prob:F4}");
            }

            return sorted.ToList();

            //Insert above
        }


        private (long[], long[]) Tokenize(string sentence, string label)
        {


            // Define the sentence and hypothesis
            string hypothesis = $"This example is {label}.";
            string fullInput = $"{sentence} [SEP] {hypothesis}";

            // Tokenize and encode the input
            var encoded = tokenizer.EncodeBatch([fullInput]);

            // Extract inputIds and attentionMask
            var input = new EmbeddingModelInput
            {
                InputIds = encoded.SelectMany(t => t.InputIds.Select(x => x)).ToArray(),
                AttentionMask = encoded.SelectMany(t => t.AttentionMask).ToArray(),
                TokenTypeIds = encoded.SelectMany(t => t.TokenTypeIds).ToArray()
            };


            return (input.InputIds, input.AttentionMask);

        }


    }
}
