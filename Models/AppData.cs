using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Models
{
    public class AppData
    {
        public InMemoryVectorStore? KnowledgeBaseVectors { get; set; }
        public int KnowledgeBaseCount { get; set; }
    }

    public class KnowledgeBaseData
    {
        [VectorStoreRecordKey]
        public required string Key { get; init; }

        [VectorStoreRecordData]
        public required string Question { get; set; }

        [VectorStoreRecordData]
        public required string Text { get; init; }

        [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
        public required ReadOnlyMemory<float> Vector { get; init; }
    }
}
