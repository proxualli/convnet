using System.Collections.Generic;
using System.Globalization;

using Float = System.Single;
using size_t = System.UInt64;

namespace ScriptsDialog
{
    public class ScriptCatalog
    {
        public const string nwl = "\r\n";

        public static string to_string(bool variable)
        {
            return variable ? "Yes" : "No";
        }

        public static string to_string(size_t number)
        {
            return number.ToString();
        }

        public static string to_string(Float number)
        {
            return number.ToString(new CultureInfo("en-US"));
        }

        public static string to_string(Datasets dataset)
        {
            return dataset.ToString();
        }

        public static string to_string(Fillers filler)
        {
            return filler.ToString();
        }

        public static size_t DIV8(size_t channels)
        {
            if (channels % 8ul == 0ul)
                return channels;

            return ((channels / 8ul) + 1ul) * 8ul;
        }

        public static size_t GetKernel(size_t index)
        {
            size_t kernel = 1ul;
            for (size_t k = 0ul; k < index; k++)
                kernel += 2;

            return kernel;
        }

        public static string In(string prefix, size_t id)
        {
            return prefix + to_string(id);
        }

        public static string BatchNorm(size_t id, string inputs, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=BatchNorm" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormActivation(size_t id, string inputs, bool relu = true, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
              (relu ? "Type=BatchNormRelu" + nwl : "Type=BatchNormFTS" + nwl) +
              "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormActivationDropout(size_t id, string inputs, bool relu = true, Float dropout = 0.0f, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
              (relu ? "Type=BatchNormReluDropout" + nwl : "Type=BatchNormFTSDropout" + nwl) +
              "Inputs=" + inputs + nwl +
              (dropout > 0f ? "Dropout=" + to_string(dropout) + nwl + nwl : nwl);
        }

        public static string BatchNormHardSwish(size_t id, string inputs, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
              "Type=BatchNormHardSwish" + nwl +
              "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormRelu(size_t id, string inputs, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=BatchNormRelu" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormReluDropout(size_t id, string inputs, Float dropout = 0.0f, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=BatchNormReluDropout" + nwl +
               "Inputs=" + inputs + nwl +
               (dropout > 0f ? "Dropout=" + to_string(dropout) + nwl + nwl : nwl);
        }

        public static string Convolution(size_t id, string inputs, size_t channels, size_t kernelX = 3, size_t kernelY = 3, size_t strideX = 1, size_t strideY = 1, size_t padX = 1, size_t padY = 1, string group = "", string prefix = "C", string weightsFiller = "")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
                "Type=Convolution" + nwl +
                "Inputs=" + inputs + nwl +
                "Channels=" + to_string(channels) + nwl +
                "Kernel=" + to_string(kernelX) + "," + to_string(kernelY) + nwl +
                (strideX != 1 || strideY != 1 ? "Stride=" + to_string(strideX) + "," + to_string(strideY) + nwl : "") +
                (padX != 0 || padY != 0 ? "Pad=" + to_string(padX) + "," + to_string(padY) + nwl : "") +
                (weightsFiller != "" ? "WeightsFiller=" + weightsFiller + nwl + nwl : nwl);
        }

        public static string DepthwiseConvolution(size_t id, string inputs, size_t multiplier = 1, size_t kernelX = 3, size_t kernelY = 3, size_t strideX = 1, size_t strideY = 1, size_t padX = 1, size_t padY = 1, string group = "", string prefix = "DC", string weightsFiller = "")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
                "Type=DepthwiseConvolution" + nwl +
                "Inputs=" + inputs + nwl +
                (multiplier > 1 ? "Multiplier=" + to_string(multiplier) + nwl : "") +
                "Kernel=" + to_string(kernelX) + "," + to_string(kernelY) + nwl +
                (strideX != 1 || strideY != 1 ? "Stride=" + to_string(strideX) + "," + to_string(strideY) + nwl : "") +
                (padX != 0 || padY != 0 ? "Pad=" + to_string(padX) + "," + to_string(padY) + nwl : "") +
                (weightsFiller != "" ? "WeightsFiller=" + weightsFiller + nwl + nwl : nwl);
        }

        public static string PartialDepthwiseConvolution(size_t id, string inputs, size_t part = 1, size_t groups = 1, size_t kernelX = 3, size_t kernelY = 3, size_t strideX = 1, size_t strideY = 1, size_t padX = 1, size_t padY = 1, string group = "", string prefix = "DC", string weightsFiller = "")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
                "Type=PartialDepthwiseConvolution" + nwl +
                "Inputs=" + inputs + nwl +
                "Group=" + to_string(part) + nwl +
                "Groups=" + to_string(groups) + nwl +
                "Kernel=" + to_string(kernelX) + "," + to_string(kernelY) + nwl +
                (strideX != 1 || strideY != 1 ? "Stride=" + to_string(strideX) + "," + to_string(strideY) + nwl : "") +
                 (padX != 0 || padY != 0 ? "Pad=" + to_string(padX) + "," + to_string(padY) + nwl : "") +
                (weightsFiller != "" ? "WeightsFiller=" + weightsFiller + nwl + nwl : nwl);
        }

        public static string DepthwiseMixedConvolution(size_t g, size_t id, string inputs, size_t strideX = 1, size_t strideY = 1, bool useChannelSplit = true, string group = "", string prefix = "DC")
        {
            switch (g)
            {
                case 0:
                    return DepthwiseConvolution(id, inputs, 1, 3, 3, strideX, strideY, 1, 1, group, prefix);

                case 1:
                    return useChannelSplit ? ChannelSplit(id, inputs, 2, 1, "Q1") + ChannelSplit(id, inputs, 2, 2, "Q2") +
                        DepthwiseConvolution(id, In("Q1CS", id), 1, 3, 3, strideX, strideY, 1, 1, "A") + DepthwiseConvolution(id, In("Q2CS", id), 1, 5, 5, strideX, strideY, 2, 2, "B") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id), group, prefix) :
                        PartialDepthwiseConvolution(id, inputs, 1, 2, 3, 3, strideX, strideY, 1, 1, "A") + PartialDepthwiseConvolution(id, inputs, 2, 2, 5, 5, strideX, strideY, 2, 2, "B") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id), group, prefix);
                /*
                case 2:
                    return useChannelSplit ? ChannelSplit(id, inputs, 3, 1, "Q1") + ChannelSplit(id, inputs, 3, 2, "Q2") + ChannelSplit(id, inputs, 3, 3, "Q3") +
                        DepthwiseConvolution(id, In("Q1CS", id), 1, 3, 3, strideX, strideY, 1, 1, "A") + DepthwiseConvolution(id, In("Q2CS", id), 1, 5, 5, strideX, strideY, 2, 2, "B") + DepthwiseConvolution(id, In("Q3CS", id), 1, 7, 7, strideX, strideY, 3, 3, "C") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id) + "," + In("CDC", id), group, prefix) :
                        PartialDepthwiseConvolution(id, inputs, 1, 3, 3, 3, strideX, strideY, 1, 1, "A") + PartialDepthwiseConvolution(id, inputs, 2, 3, 5, 5, strideX, strideY, 2, 2, "B") +
                        PartialDepthwiseConvolution(id, inputs, 3, 3, 7, 7, strideX, strideY, 3, 3, "C") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id) + "," + In("CDC", id), group, prefix);
                */
                default:
                    return useChannelSplit ? ChannelSplit(id, inputs, 4, 1, "Q1") + ChannelSplit(id, inputs, 4, 2, "Q2") + ChannelSplit(id, inputs, 4, 3, "Q3") + ChannelSplit(id, inputs, 4, 4, "Q4") +
                        DepthwiseConvolution(id, In("Q1CS", id), 1, 3, 3, strideX, strideY, 1, 1, "A") + DepthwiseConvolution(id, In("Q2CS", id), 1, 5, 5, strideX, strideY, 2, 2, "B") +
                        DepthwiseConvolution(id, In("Q3CS", id), 1, 7, 7, strideX, strideY, 3, 3, "C") + DepthwiseConvolution(id, In("Q4CS", id), 1, 9, 9, strideX, strideY, 4, 4, "D") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id) + "," + In("CDC", id) + "," + In("DDC", id), group, prefix) :
                        PartialDepthwiseConvolution(id, inputs, 1, 4, 3, 3, strideX, strideY, 1, 1, "A") + PartialDepthwiseConvolution(id, inputs, 2, 4, 5, 5, strideX, strideY, 2, 2, "B") +
                        PartialDepthwiseConvolution(id, inputs, 3, 4, 7, 7, strideX, strideY, 3, 3, "C") + PartialDepthwiseConvolution(id, inputs, 4, 4, 9, 9, strideX, strideY, 4, 4, "D") +
                        Concat(id, In("ADC", id) + "," + In("BDC", id) + "," + In("CDC", id) + "," + In("DDC", id), group, prefix);
            }
        }

        public static string ChannelSplit(size_t id, string inputs, size_t groups, size_t part, string group = "", string prefix = "CS")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=ChannelSplit" + nwl +
               "Inputs=" + inputs + nwl +
               "Groups=" + to_string(groups) + nwl +
               "Group=" + to_string(part) + nwl + nwl;
        }

        public static string ChannelShuffle(size_t id, string inputs, size_t groups = 2, string group = "", string prefix = "CSH")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=ChannelShuffle" + nwl +
               "Inputs=" + inputs + nwl +
               "Groups=" + to_string(groups) + nwl + nwl;
        }

        public static string Concat(size_t id, string inputs, string group = "", string prefix = "CC")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Concat" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string GlobalAvgPooling(string input, string group = "", string prefix = "GAP")
        {
            return "[" + group + prefix + "]" + nwl +
                "Type=GlobalAvgPooling" + nwl +
                "Inputs=" + input + nwl + nwl;
        }

        public static string Add(size_t id, string inputs, string group = "", string prefix = "A")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Add" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string ChannelMultiply(string inputs, string group = "", string prefix = "CM")
        {
            return "[" + group + prefix + "]" + nwl +
               "Type=ChannelMultiply" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string Dropout(size_t id, string inputs, string group = "", string prefix = "D")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Dropout" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string HardLogistic(size_t id, string inputs, string group = "", string prefix = "ACT")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Activation" + nwl +
               "Inputs=" + inputs + nwl +
               "Activation=HardLogistic" + nwl + nwl;
        }

        public static string HardSwish(size_t id, string inputs, string group = "", string prefix = "ACT")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Activation" + nwl +
               "Inputs=" + inputs + nwl +
               "Activation=HardSwish" + nwl + nwl;
        }

        internal static string Generate(ScriptParameters p)
        {
            var net =
                "[" + p.ModelName + "]" + nwl +
                "Dataset=" + to_string(p.Dataset) + nwl +
                "Dim=" + to_string(p.C) + "," + to_string(p.H) + "," + to_string(p.W) + nwl +
                ((p.PadH > 0 || p.PadW > 0) ? (!p.MirrorPad ? "ZeroPad=" + to_string(p.PadH) + "," + to_string(p.PadW) + nwl : "MirrorPad=" + to_string(p.PadH) + "," + to_string(p.PadW) + nwl) : "") +
                ((p.PadH > 0 || p.PadW > 0) ? "RandomCrop=Yes" + nwl : "") +
                "WeightsFiller=" + to_string(p.WeightsFiller) + (p.WeightsScaleVisible ? "(" + to_string(p.WeightsScale) + ")" : "") + nwl +
                (p.WeightsLRM != 1 ? "WeightsLRM=" + to_string(p.WeightsLRM) + nwl : "") +
                (p.WeightsWDM != 1 ? "WeightsWDM=" + to_string(p.WeightsWDM) + nwl : "") +
                (p.HasBias ? "BiasesFiller=" + to_string(p.BiasesFiller) + (p.BiasesScaleVisible ? "(" + to_string(p.BiasesScale) + ")" : "") + nwl +
                (p.BiasesLRM != 1 ? "BiasesLRM=" + to_string(p.BiasesLRM) + nwl : "") +
                (p.BiasesWDM != 1 ? "BiasesWDM=" + to_string(p.BiasesWDM) + nwl : "") : "Biases=No" + nwl) +
                (p.DropoutVisible ? "Dropout=" + to_string(p.Dropout) + nwl : "") +
                "Scaling=" + to_string(p.BatchNormScaling) + nwl +
                "Momentum=" + to_string(p.BatchNormMomentum) + nwl +
                "Eps=" + to_string(p.BatchNormEps) + nwl + nwl;

            var blocks = new List<string>();

            switch (p.Script)
            {
                case Scripts.densenet:
                    {
                        var channels = DIV8(p.GrowthRate);

                        net += Convolution(1, "Input", channels, 3, 3, 1, 1, 1, 1);

                        if (p.Bottleneck)
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Relu) +
                                Convolution(2, "B1", DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                BatchNormActivation(2, "C2", p.Relu) +
                                Convolution(3, "B2", DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? Dropout(3, "C3") + Concat(1, "C1,D3") : Concat(1, "C1,C3")));
                        }
                        else
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Relu) +
                                Convolution(2, "B1", DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? Dropout(2, "C2") + Concat(1, "C1,D2") : Concat(1, "C1,C2")));
                        }

                        var CC = 1ul;
                        var C = p.Bottleneck ? 4ul : 3ul;

                        channels += p.GrowthRate;

                        for (var g = 1ul; g <= p.Groups; g++)  // 32*32 16*16 8*8 or 28*28 14*14 7*7
                        {
                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("CC", CC), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C + 1, In("C", C + 1)) + Concat(CC + 1, In("CC", CC) + "," + In("D", C + 1)) : Concat(CC + 1, In("CC", CC) + "," + In("C", C + 1))));

                                    C += 2;
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("CC", CC), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C, In("C", C)) + Concat(CC + 1, In("CC", CC) + "," + In("D", C)) : Concat(CC + 1, In("CC", CC) + "," + In("C", C))));

                                    C++;
                                }

                                CC++;
                                channels += p.GrowthRate;
                            }

                            if (g < p.Groups)
                            {
                                channels = DIV8((size_t)System.Math.Floor(2.0 * channels * p.Compression));

                                if (p.Dropout > 0)
                                    blocks.Add(
                                        Convolution(C, In("CC", CC), channels, 1, 1, 1, 1, 0, 0) +
                                        Dropout(C, In("C", C)) +
                                        "[P" + to_string(g) + "]" + nwl + "Type=AvgPooling" + nwl + "Inputs=D" + to_string(C) + nwl + "Kernel=2,2" + nwl + "Stride=2,2" + nwl + nwl);
                                else
                                    blocks.Add(
                                        Convolution(C, "CC" + to_string(CC), channels, 1, 1, 1, 1, 0, 0) +
                                        "[P" + to_string(g) + "]" + nwl + "Type=AvgPooling" + nwl + "Inputs=C" + to_string(C) + nwl + "Kernel=2,2" + nwl + "Stride=2,2" + nwl + nwl);
                                C++;
                                CC++;

                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("P", g), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C + 1, In("C", C + 1)) + Concat(CC, In("B", C) + "," + In("D", C + 1)) : Concat(CC, In("B", C) + "," + In("C", C + 1))));

                                    C += 2;
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("P", g), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C, In("C", C)) + Concat(CC, In("B", C) + "," + In("D", C)) : Concat(CC, In("B", C) + "," + In("C", C))));

                                    C++;
                                }

                                channels += p.GrowthRate;
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            Convolution(C, In("CC", CC), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            "[ACT]" + nwl + "Type=Activation" + nwl + "Inputs=GAP" + nwl + "Activation=LogSoftmax" + nwl + nwl +
                            "[Cost]" + nwl + "Type=Cost" + nwl + "Inputs=ACT" + nwl + "Cost=CategoricalCrossEntropy" + nwl + "Channels=" + to_string(p.Classes);
                    }
                    break;

                case Scripts.mobilenetv3:
                    {
                        var se = p.Relu ? false : p.SqueezeExcitation;
                        var channelsplit = true;
                        var W = p.Width * 16;

                        net +=
                            Convolution(1, "Input", DIV8(W), 3, 3, 1, 1, 1, 1) +
                            BatchNormActivation(1, "C1", p.Relu);

                        blocks.Add(
                            Convolution(2, "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(2, "C2", p.Relu) +
                            DepthwiseMixedConvolution(0, 3, "B2", 1, 1, channelsplit) +
                            BatchNormActivation(3, "DC3") +
                            Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNorm(4, "C4"));

                        var A = 1ul;
                        var C = 5ul;

                        for (var g = 1ul; g <= p.Groups; g++)  // 32*32 16*16 8*8 or 28*28 14*14 7*7
                        {
                            var mix = g - 1ul;

                            if (g > 1)
                            {
                                W *= 2;

                                var group = In("SE", C + 1);
                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 1), group) +
                                    Convolution(1, group + "GAP", DIV8((6 * W) / 4), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(1, group + "C1", p.Relu, group) +
                                    Convolution(2, group + "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0, group) +
                                    HardLogistic(2, group + "C2", group) +
                                    ChannelMultiply(In("B", C + 1) + "," + group + "ACT2", group) +
                                    Convolution(C + 2, group + "CM", DIV8(W), 1, 1, 1, 1, 0, 0) :
                                    Convolution(C + 2, In("B", C + 1), DIV8(W), 1, 1, 1, 1, 0, 0);

                                //auto strDropout = p.Dropout > 0 ? Dropout(C, In("A", A)) +
                                //    Convolution(C, In("D", C), 6 * W, 1, 1, 1, 1, 0, 0) :
                                //    Convolution(C, In("A", A), 6 * W, 1, 1, 1, 1, 0, 0);

                                blocks.Add(
                                    Convolution(C, In("A", A), DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C, In("C", C), p.Relu) +
                                    DepthwiseMixedConvolution(mix, C + 1, In("B", C), 2, 2, channelsplit) +
                                    BatchNormActivation(C + 1, In("DC", C + 1), p.Relu) +
                                    strSE +
                                    BatchNorm(C + 2, In("C", C + 2)));

                                C += 3;
                            }

                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                var strOutputLayer = (i == 1 && g > 1) ? In("B", C - 1) : (i == 1 && g == 1) ? "B4" : In("A", A);

                                var group = In("SE", C + 1);

                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 1), group) +
                                    Convolution(1, group + "GAP", DIV8((6 * W) / 4), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(1, group + "C1", p.Relu, group) +
                                    Convolution(2, group + "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0, group) +
                                    HardLogistic(2, group + "C2", group) +
                                    ChannelMultiply(In("B", C + 1) + "," + group + "ACT2", group) +
                                    Convolution(C + 2, group + "CM", DIV8(W), 1, 1, 1, 1, 0, 0) :
                                    Convolution(C + 2, In("B", C + 1), DIV8(W), 1, 1, 1, 1, 0, 0);

                                blocks.Add(
                                    Convolution(C, strOutputLayer, DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C, In("C", C), p.Relu) +
                                    DepthwiseMixedConvolution(mix, C + 1, In("B", C), 1, 1, channelsplit) +
                                    BatchNormActivation(C + 1, In("DC", C + 1), p.Relu) +
                                    strSE +
                                    BatchNorm(C + 2, In("C", C + 2)) +
                                    Add(A + 1, In("B", C + 2) + "," + strOutputLayer));

                                A++;
                                C += 3;
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            BatchNormActivation(C, In("A", A), p.Relu) +
                            Convolution(C, In("B", C), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            "[ACT]" + nwl + "Type=Activation" + nwl + "Inputs=GAP" + nwl + "Activation=LogSoftmax" + nwl + nwl +
                            "[Cost]" + nwl + "Type=Cost" + nwl + "Inputs=ACT" + nwl + "Cost=CategoricalCrossEntropy" + nwl + "Channels=" + to_string(p.Classes);
                    }
                    break;

                case Scripts.resnet:
                    {
                        var bn = p.Bottleneck ? 1ul : 0ul;
                        const Float K = 2;
                        var W = p.Width * 16;
                        var A = 1ul;
                        var C = 5ul;

                        net += Convolution(1, "Input", DIV8(W), 3, 3, 1, 1, 1, 1);

                        if (p.Bottleneck)
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Relu) +
                                Convolution(2, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                BatchNormActivation(2, "C2", p.Relu) +
                                Convolution(3, "B2", DIV8((size_t)(K * W / 4)), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? BatchNormActivationDropout(3, "C3") : BatchNormActivation(3, "C3", p.Relu)) +
                                Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Convolution(5, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Add(1, "C4,C5"));

                            C = 6;
                        }
                        else
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Relu) +
                                Convolution(2, "B1", DIV8(W), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? BatchNormActivationDropout(2, "C2") : BatchNormActivation(2, "C2", p.Relu)) +
                                Convolution(3, "B2", DIV8(W), 3, 3, 1, 1, 1, 1) +
                                Convolution(4, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Add(1, "C3,C4"));
                        }

                        for (var g = 0ul; g < p.Groups; g++)  // 32*32 16*16 8*8 or 28*28 14*14 7*7
                        {
                            if (g > 0)
                            {
                                W *= 2;

                                var strChannelZeroPad = p.ChannelZeroPad ?
                                    ("[AVG" + to_string(g) + "]" + nwl + "Type=AvgPooling" + nwl + "Inputs=A" + to_string(A) + nwl + "Kernel=3,3" + nwl + "Stride=2,2" + nwl + "Pad=1,1" + nwl + nwl +
                                    "[CZP" + to_string(g) + "]" + nwl + "Type=ChannelZeroPad" + nwl + "Inputs=AVG" + to_string(g) + nwl + "Channels=" + to_string(W) + nwl + nwl +
                                    Add(A + 1, In("C", C + 1 + bn) + "," + In("CZP", g))) :
                                    "[AVG" + to_string(g) + "]" + nwl + "Type=AvgPooling" + nwl + "Inputs=B" + to_string(C) + nwl + "Kernel=2,2" + nwl + "Stride=2,2" + nwl + nwl +
                                    (Convolution(C + 2 + bn, In("AVG", g), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    Add(A + 1, In("C", C + 1 + bn) + "," + In("C", C + 2 + bn)));

                                if (p.Bottleneck)
                                {

                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 2, 2, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 2, In("C", C + 1)) : BatchNormActivation(C + 2, In("C", C + 1), p.Relu)) +
                                        Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        strChannelZeroPad);
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(W), 3, 3, 2, 2, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 1, In("C", C)) : BatchNormActivation(C + 1, In("C", C), p.Relu)) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        strChannelZeroPad);
                                }

                                A++;
                                if (p.ChannelZeroPad)
                                    C += 2 + bn;
                                else
                                    C += 3 + bn;
                            }

                            for (var i = 1u; i < p.Iterations; i++)
                            {
                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                        Convolution(C + 1, In("B", C + 1), DIV8((size_t)(K * W / 4)), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 2, In("C", C + 1)) : BatchNormActivation(C + 2, In("C", C + 1), p.Relu)) +
                                        Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        Add(A + 1, In("C", C + 2) + "," + In("A", A)));
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Relu) +
                                        Convolution(C, In("B", C), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 1, In("C", C)) : BatchNormActivation(C + 1, In("C", C), p.Relu)) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        Add(A + 1, In("C", C + 1) + "," + In("A", A)));
                                }

                                A++; C += 2 + bn;
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            BatchNormActivation(C, In("A", A), p.Relu) +
                            Convolution(C, In("B", C), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            "[ACT]" + nwl + "Type=Activation" + nwl + "Inputs=GAP" + nwl + "Activation=LogSoftmax" + nwl + nwl +
                            "[Cost]" + nwl + "Type=Cost" + nwl + "Inputs=ACT" + nwl + "Cost=CategoricalCrossEntropy" + nwl + "Channels=" + to_string(p.Classes);
                    }
                    break;

                case Scripts.shufflenetv2:
                    {
                        var se = false;
                        var W = p.Width * 16;
                        var kernel = 3ul;
                        var pad = 1ul;

                        net += Convolution(1, "Input", DIV8(W), kernel, kernel, 1, 1, pad, pad);

                        blocks.Add(
                            BatchNormActivation(1, "C1", p.Relu) +
                            Convolution(2, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(2, "C2", p.Relu) +
                            DepthwiseConvolution(3, "B2", 1, kernel, kernel, 1, 1, pad, pad) +
                            BatchNorm(3, "DC3") +
                            Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(4, "C4", p.Relu) +
                            Convolution(5, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            Concat(1, "C5,B4"));

                        var C = 6ul;
                        var A = 1ul;

                        for (var g = 1ul; g <= p.Groups; g++)  // 32*32 16*16 8*8 or 28*28 14*14 7*7
                        {
                            if (g > 1)
                            {
                                se = p.SqueezeExcitation;
                                W *= 2;

                                blocks.Add(
                                    Convolution(C, In("CC", A), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                    DepthwiseConvolution(C + 1, In("B", C + 1), 1, kernel, kernel, 2, 2, pad, pad) +
                                    BatchNorm(C + 2, In("DC", C + 1)) +
                                    Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 3, In("C", C + 2), p.Relu) +
                                    DepthwiseConvolution(C + 3, In("CC", A), 1, kernel, kernel, 2, 2, pad, pad) +
                                    BatchNorm(C + 4, In("DC", C + 3)) +
                                    Convolution(C + 4, In("B", C + 4), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 5, In("C", C + 4), p.Relu) +
                                    Concat(A + 1, In("B", C + 5) + "," + In("B", C + 3)));

                                A++; C += 5;
                            }

                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                var group = In("SE", C + 3);
                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 3), group) +
                                    Convolution(1, group + "GAP", DIV8(W / 4), 1, 1, 1, 1, 0, 0, group, "C", "Normal(0.01)") +
                                    BatchNormActivation(1, group + "C1", p.Relu, group) +
                                    Convolution(2, group + "B1", DIV8(W), 1, 1, 1, 1, 0, 0, group, "C", "Normal(0.01)") +
                                    HardLogistic(2, group + "C2", group) +
                                    ChannelMultiply(In("B", C + 3) + "," + group + "ACT2", group) +
                                    Concat(A + 1, In("LCS", A) + "," + group + "CM") :
                                    Concat(A + 1, In("LCS", A) + "," + In("B", C + 3));

                                blocks.Add(
                                    ChannelShuffle(A, In("CC", A), 2) +
                                    ChannelSplit(A, In("CSH", A), 2, 1, "L") + ChannelSplit(A, In("CSH", A), 2, 2, "R") +
                                    Convolution(C, In("RCS", A), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 1, In("C", C), p.Relu) +
                                    DepthwiseConvolution(C + 1, In("B", C + 1), 1, kernel, kernel, 1, 1, pad, pad) +
                                    BatchNorm(C + 2, In("DC", C + 1)) +
                                    Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 3, In("C", C + 2), p.Relu) +
                                    strSE);

                                A++; C += 3;
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            Convolution(C, In("CC", A), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            "[ACT]" + nwl + "Type=Activation" + nwl + "Inputs=GAP" + nwl + "Activation=LogSoftmax" + nwl + nwl +
                            "[Cost]" + nwl + "Type=Cost" + nwl + "Inputs=ACT" + nwl + "Cost=CategoricalCrossEntropy" + nwl + "Channels=" + to_string(p.Classes);
                    }
                    break;
            }

            return net;
        }
    }
}
