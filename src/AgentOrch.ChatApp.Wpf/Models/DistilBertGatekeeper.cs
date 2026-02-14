/*

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

using Tokenizers.HuggingFace.Tokenizer;





public sealed class DistilBertGatekeeper
{

    private const string _onnx = "F:\\AI-Models\\distilbert-base-uncased-finetuned-sst-2-english\\onnx\\model.onnx";
    private const string _token = "F:\\AI-Models\\distilbert-base-uncased-finetuned-sst-2-english\\onnx\\tokenizer.json";
    private readonly string[] _labels;
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;








    public DistilBertGatekeeper(string[] labels)
    {
        _session = new InferenceSession(_onnx);
        _tokenizer = Tokenizer.FromFile(_token);
        _labels = labels;
    }








    private (DenseTensor<long> inputIds, DenseTensor<long> attentionMask) Encode(string text)
    {
        //  var encoding = _tokenizer.Encode(text);

        //      var inputIds = encoding[0];
        //     var attentionMask = encoding;

        //      var inputIdsTensor = new DenseTensor<long>(new[] { 1, inputIds.Count });
        //      var attentionMaskTensor = new DenseTensor<long>(new[] { 1, attentionMask.Count });
        /*
              for (var i = 0; i < inputIds.Count; i++)
              {
                  inputIdsTensor[0, i] = inputIds[i];
                  attentionMaskTensor[0, i] = attentionMask[i];
              }

              return (inputIdsTensor, attentionMaskTensor);


        return (null, null);
    }








    private float[] RunInference(DenseTensor<long> inputIds, DenseTensor<long> attentionMask)
    {
        var inputs = new List<NamedOnnxValue>
        {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        };

        using var results = _session.Run(inputs);

        return results
                .First(r => r.Name == "logits")
                .AsEnumerable<float>()
                .ToArray();
    }








    private static int ArgMax(float[] values)
    {
        var max = values[0];
        var index = 0;

        for (var i = 1; i < values.Length; i++)
            if (values[i] > max)
            {
                max = values[i];
                index = i;
            }

        return index;
    }








    public string Classify(string text)
    {
        var (inputIds, attentionMask) = Encode(text);

        var logits = RunInference(inputIds, attentionMask);

        var predictedIndex = ArgMax(logits);

        return _labels[predictedIndex];
    }
}*/



