using System.Collections.Generic;
using System.Globalization;

using Float = System.Single;
using UInt = System.UInt64;

namespace ScriptsDialog
{
    public class ScriptCatalog
    {
        public const string nwl = "\r\n";

        public static string to_string(bool variable)
        {
            return variable ? "Yes" : "No";
        }

        public static string to_string(UInt number)
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

        public static UInt DIV8(UInt channels)
        {
            if (channels % 8ul == 0ul)
                return channels;

            return ((channels / 8ul) + 1ul) * 8ul;
        }

        public static string In(string prefix, UInt id)
        {
            return prefix + to_string(id);
        }

        public static string BatchNorm(UInt id, string inputs, string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=BatchNorm" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormActivation(UInt id, string inputs, string activation = "Relu", string group = "", string prefix = "B")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
              "Type=BatchNorm" + activation + nwl +
              "Inputs=" + inputs + nwl + nwl;
        }

        public static string BatchNormActivation(UInt id, string inputs, Activations activation = Activations.Relu, string group = "", string prefix = "B")
        {
            if (activation != Activations.FRelu)
            {
                return "[" + group + prefix + to_string(id) + "]" + nwl +
                      "Type=BatchNorm" + activation.ToString() + nwl +
                      "Inputs=" + inputs + nwl + nwl;
            }
            else
            {
                return "[" + group + "B" + to_string(id) + "B1]" + nwl +
                    "Type=BatchNorm" + nwl +
                    "Inputs=" + inputs + nwl + nwl +

                    "[" + group + "DC" + to_string(id) + "DC]" + nwl +
                    "Type=DepthwiseConvolution" + nwl +
                    "Inputs=" + group + "B" + to_string(id) + "B1" + nwl +
                    "Kernel=3,3" + nwl +
                    "Pad=1,1" + nwl + nwl +

                    "[" + group + "B" + to_string(id) + "B2]" + nwl +
                    "Type=BatchNorm" + nwl +
                    "Inputs=" + group + "DC" + to_string(id) + "DC" + nwl + nwl +

                    "[" + group + prefix + to_string(id) + "]" + nwl +
                    "Type=Max" + nwl +
                    "Inputs=" + group + "B" + to_string(id) + "B2," + group + "B" + to_string(id) + "B1" + nwl + nwl;
            }
        }

        public static string BatchNormActivationDropout(UInt id, string inputs, Activations activation = Activations.Relu, Float dropout = 0.0f, string group = "", string prefix = "B")
        {
            if (activation != Activations.FRelu)
            {
                return "[" + group + prefix + to_string(id) + "]" + nwl +
                    "Type=BatchNorm" + activation.ToString() + "Dropout" + nwl +
                    "Inputs=" + inputs + nwl +
                      (dropout > 0f ? "Dropout=" + to_string(dropout) + nwl + nwl : nwl);
            }
            else
            {
                return "[" + group + prefix + to_string(id) + "]" + nwl +
                   "Type=BatchNormHardSwishDropout" + nwl +
                   "Inputs=" + inputs + nwl +
                     (dropout > 0f ? "Dropout=" + to_string(dropout) + nwl + nwl : nwl);
            }
        }

        public static string Resampling(UInt id, string inputs, string group = "", string prefix = "R")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Resampling" + nwl +
               "Inputs=" + inputs + nwl +
               "Factor=0.5,0.5" + nwl +
               "Algorithm=Linear" + nwl + nwl;
        }

        public static string Convolution(UInt id, string inputs, UInt channels, UInt kernelX = 3, UInt kernelY = 3, UInt strideX = 1, UInt strideY = 1, UInt padX = 1, UInt padY = 1, string group = "", string prefix = "C", string weightsFiller = "")
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

        public static string DepthwiseConvolution(UInt id, string inputs, UInt multiplier = 1, UInt kernelX = 3, UInt kernelY = 3, UInt strideX = 1, UInt strideY = 1, UInt padX = 1, UInt padY = 1, string group = "", string prefix = "DC", string weightsFiller = "")
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

        public static string PartialDepthwiseConvolution(UInt id, string inputs, UInt part = 1, UInt groups = 1, UInt kernelX = 3, UInt kernelY = 3, UInt strideX = 1, UInt strideY = 1, UInt padX = 1, UInt padY = 1, string group = "", string prefix = "DC", string weightsFiller = "")
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

        public static string DepthwiseMixedConvolution(UInt g, UInt id, string inputs, UInt strideX = 1, UInt strideY = 1, bool useChannelSplit = true, string group = "", string prefix = "DC")
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

        public static string ChannelSplit(UInt id, string inputs, UInt groups, UInt part, string group = "", string prefix = "CS")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=ChannelSplit" + nwl +
               "Inputs=" + inputs + nwl +
               "Groups=" + to_string(groups) + nwl +
               "Group=" + to_string(part) + nwl + nwl;
        }

        public static string ChannelShuffle(UInt id, string inputs, UInt groups = 2, string group = "", string prefix = "CSH")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=ChannelShuffle" + nwl +
               "Inputs=" + inputs + nwl +
               "Groups=" + to_string(groups) + nwl + nwl;
        }

        public static string Concat(UInt id, string inputs, string group = "", string prefix = "CC")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Concat" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string AvgPooling(UInt id, string input, string kernel = "3,3", string stride = "2,2", string pad = "1,1", string group = "", string prefix = "P")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
                "Type=AvgPooling" + nwl +
                "Inputs=" + input + nwl +
                "Kernel=" + kernel + nwl +
                "Stride=" + stride + nwl +
                "Pad=" + pad + nwl + nwl;
        }

        public static string GlobalAvgPooling(string input, string group = "", string prefix = "GAP")
        {
            return "[" + group + prefix + "]" + nwl +
                "Type=GlobalAvgPooling" + nwl +
                "Inputs=" + input + nwl + nwl;
        }

        public static string Dense(string inputs, UInt channels, string group = "", string prefix = "DS")
        {
            return "[" + group + prefix + "]" + nwl +
               "Type=Dense" + nwl +
               "Inputs=" + inputs + nwl +
               "Channels=" + to_string(channels) + nwl + nwl;
        }

        public static string Add(UInt id, string inputs, string group = "", string prefix = "A")
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

        public static string Dropout(UInt id, string inputs, string group = "", string prefix = "D")
        {
            return "[" + group + prefix + to_string(id) + "]" + nwl +
               "Type=Dropout" + nwl +
               "Inputs=" + inputs + nwl + nwl;
        }

        public static string Activation(string inputs, string activation = "Relu", string group = "", string prefix = "ACT")
        {
            return "[" + group + prefix + "]" + nwl +
               "Type=Activation" + nwl +
               "Inputs=" + inputs + nwl +
               "Activation=" + activation + nwl + nwl;
        }

        public static string Cost(string inputs, Datasets dataset, UInt channels, string cost = "CategoricalCrossEntropy", Float eps = 0.0f, string group = "", string prefix = "Cost")
        {
            return "[" + group + prefix + "]" + nwl +
               "Type=Cost" + nwl +
               "Inputs=" + inputs + nwl +
               "Cost=" + cost + nwl +
               "LabelIndex=" + ((dataset == Datasets.cifar100 && channels == 100) ? "1" : "0") + nwl +
               "Channels=" + to_string(channels) + nwl +
               "Eps=" + to_string(eps);
        }

        public static List<string> MBConv(UInt id, string inputs, UInt inputChannels, UInt outputChannels, UInt stride = 1, UInt expandRatio = 4, bool se = false, Activations activation = Activations.HardSwish)
        {
            var blocks = new List<string>();
            var hiddenDim = DIV8(inputChannels * expandRatio);
            var identity = stride == 1 && inputChannels == outputChannels;

            if (se)
            {
                var group = In("SE", id + 1);

                blocks.Add(
                    Convolution(id, inputs, hiddenDim, 1, 1, 1, 1, 0, 0) +
                    BatchNormActivation(id, In("C", id), activation) +
                    DepthwiseConvolution(id + 1, In("B", id), 1, 3, 3, stride, stride, 1, 1) +
                    BatchNormActivation(id + 1, In("DC", id + 1), activation) +
                    GlobalAvgPooling(In("B", id + 1), group) +
                    Convolution(1, group + "GAP", DIV8(hiddenDim / expandRatio), 1, 1, 1, 1, 0, 0, group) +
                    BatchNormActivation(1, group + "C1", activation == Activations.FRelu ? Activations.HardSwish : activation, group) +
                    Convolution(2, group + "B1", hiddenDim, 1, 1, 1, 1, 0, 0, group) +
                    BatchNormActivation(2, group + "C2", "HardLogistic", group) +
                    ChannelMultiply(In("B", id + 1) + "," + group + "B2", group) +
                    Convolution(id + 2, group + "CM", DIV8(outputChannels), 1, 1, 1, 1, 0, 0) +
                    BatchNorm(id + 2, In("C", id + 2)));
            }
            else
            {
                blocks.Add(
                    Convolution(id, inputs, hiddenDim, 1, 1, 1, 1, 0, 0) +
                    BatchNormActivation(id, In("C", id), activation) +
                    DepthwiseConvolution(id + 1, In("B", id), 1, 3, 3, stride, stride, 1, 1) +
                    BatchNormActivation(id + 1, In("DC", id + 1), activation) +
                    Convolution(id + 2, In("B", id + 1), DIV8(outputChannels), 1, 1, 1, 1, 0, 0) +
                    BatchNorm(id + 2, In("C", id + 2)));
            }

            if (identity)
                blocks.Add(Add(id + 2, In("B", id + 2) + "," + inputs));

            return blocks;
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
                                BatchNormActivation(1, "C1", p.Activation) +
                                Convolution(2, "B1", DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                BatchNormActivation(2, "C2", p.Activation) +
                                Convolution(3, "B2", DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? Dropout(3, "C3") + Concat(1, "C1,D3") : Concat(1, "C1,C3")));
                        }
                        else
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Activation) +
                                Convolution(2, "B1", DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? Dropout(2, "C2") + Concat(1, "C1,D2") : Concat(1, "C1,C2")));
                        }

                        var CC = 1ul;
                        var C = p.Bottleneck ? 4ul : 3ul;

                        channels += DIV8(p.GrowthRate);

                        for (var g = 1ul; g <= p.Groups; g++)
                        {
                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("CC", CC), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C + 1, In("C", C + 1)) + Concat(CC + 1, In("CC", CC) + "," + In("D", C + 1)) : Concat(CC + 1, In("CC", CC) + "," + In("C", C + 1))));

                                    C += 2;
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("CC", CC), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C, In("C", C)) + Concat(CC + 1, In("CC", CC) + "," + In("D", C)) : Concat(CC + 1, In("CC", CC) + "," + In("C", C))));

                                    C++;
                                }

                                CC++;
                                channels += DIV8(p.GrowthRate);
                            }

                            if (g < p.Groups)
                            {
                                channels = DIV8((UInt)System.Math.Floor(2.0 * channels * p.Compression));

                                if (p.Dropout > 0)
                                    blocks.Add(
                                        Convolution(C, In("CC", CC), channels, 1, 1, 1, 1, 0, 0) +
                                        Dropout(C, In("C", C)) +
                                        AvgPooling(g, In("D", C), "2,2", "2,2", "0,0"));
                                else
                                    blocks.Add(
                                        Convolution(C, "CC" + to_string(CC), channels, 1, 1, 1, 1, 0, 0) +
                                        AvgPooling(g, In("C", C), "2,2", "2,2", "0,0"));
                                C++;
                                CC++;

                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("P", g), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(4 * p.GrowthRate), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C + 1, In("C", C + 1)) + Concat(CC, In("B", C) + "," + In("D", C + 1)) : Concat(CC, In("B", C) + "," + In("C", C + 1))));

                                    C += 2;
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("P", g), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(p.GrowthRate), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? Dropout(C, In("C", C)) + Concat(CC, In("B", C) + "," + In("D", C)) : Concat(CC, In("B", C) + "," + In("C", C))));

                                    C++;
                                }

                                channels += DIV8(p.GrowthRate);
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            Convolution(C, In("CC", CC), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            Activation("GAP", "LogSoftmax") +
                            Cost("ACT", p.Dataset, p.Classes, "CategoricalCrossEntropy", 0.125f);
                    }
                    break;

                case Scripts.efficientnetv2:
                    {
                        var inputChannels = DIV8(p.EffNet[0].Channels);
                        var C = 1ul;

                        net +=
                           Convolution(C, "Input", inputChannels, 3, 3, 1, 1, 1, 1) +
                           BatchNormActivation(C, In("C", C), p.Activation);

                        var inp = In("B", C);
                        C++;
                        foreach (var rec in p.EffNet)
                        {
                            var outputChannels = DIV8(rec.Channels);
                            for (var n = 0ul; n < rec.Iterations; n++)
                            {
                                var stride = n == 0ul ? rec.Stride : 1ul;
                                var identity = stride == 1ul && inputChannels == outputChannels;
                                var subblocks = MBConv(C, inp, inputChannels, outputChannels, stride, rec.ExpandRatio, rec.SE, p.Activation);

                                foreach (var blk in subblocks)
                                    net += blk;

                                inputChannels = outputChannels;
                                C += 2;

                                inp = In((identity ? "A" : "B"), C);
                                C++;
                            }
                        }

                        net +=
                           Convolution(C, In("A", C - 1), inputChannels, 1, 1, 1, 1, 0, 0) +
                           BatchNormActivation(C + 1, In("C", C), p.Activation) +
                           GlobalAvgPooling(In("B", C + 1)) +
                           Dense("GAP", p.Classes) +
                           Activation("DS", "LogSoftmax") +
                           Cost("ACT", p.Dataset, p.Classes, "CategoricalCrossEntropy", 0.1f);
                    }
                    break;

                case Scripts.mobilenetv3:
                    {
                        var se = p.SqueezeExcitation;
                        var channelsplit = true;
                        var W = p.Width * 16;

                        net +=
                            Convolution(1, "Input", DIV8(W), 3, 3, 1, 1, 1, 1) +
                            BatchNormActivation(1, "C1", p.Activation);

                        blocks.Add(
                            Convolution(2, "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(2, "C2", p.Activation) +
                            DepthwiseMixedConvolution(0, 3, "B2", 1, 1, channelsplit) +
                            BatchNormActivation(3, "DC3", p.Activation) +
                            Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNorm(4, "C4"));

                        var A = 1ul;
                        var C = 5ul;

                        for (var g = 1ul; g <= p.Groups; g++)
                        {
                            var mix = 0ul; // g - 1ul;

                            if (g > 1)
                            {
                                W *= 2;

                                var group = In("SE", C + 1);
                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 1), group) +
                                    Convolution(1, group + "GAP", DIV8((6 * W) / 4), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(1, group + "C1", p.Activation == Activations.FRelu ? Activations.HardSwish : p.Activation, group) +
                                    Convolution(2, group + "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(2, group + "C2", "HardLogistic", group) +
                                    ChannelMultiply(In("B", C + 1) + "," + group + "B2", group) +
                                    Convolution(C + 2, group + "CM", DIV8(W), 1, 1, 1, 1, 0, 0) :
                                    Convolution(C + 2, In("B", C + 1), DIV8(W), 1, 1, 1, 1, 0, 0);

                                blocks.Add(
                                    Convolution(C, In("A", A), DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C, In("C", C), p.Activation) +
                                    DepthwiseMixedConvolution(mix, C + 1, In("B", C), 2, 2, channelsplit) +
                                    BatchNormActivation(C + 1, In("DC", C + 1), p.Activation) +
                                    strSE +
                                    BatchNorm(C + 2, In("C", C + 2)));

                                C += 3;
                            }

                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                var strOutputLayer = (i == 1 && g > 1) ? In("B", C - 1) : (i == 1 && g == 1) ? In("B", 4) : In("A", A);

                                var group = In("SE", C + 1);

                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 1), group) +
                                    Convolution(1, group + "GAP", DIV8((6 * W) / 4), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(1, group + "C1", p.Activation == Activations.FRelu ? Activations.HardSwish : p.Activation, group) +
                                    Convolution(2, group + "B1", DIV8(6 * W), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(2, group + "C2", "HardLogistic", group) +
                                    ChannelMultiply(In("B", C + 1) + "," + group + "B2", group) +
                                    Convolution(C + 2, group + "CM", DIV8(W), 1, 1, 1, 1, 0, 0) :
                                    Convolution(C + 2, In("B", C + 1), DIV8(W), 1, 1, 1, 1, 0, 0);

                                blocks.Add(
                                    Convolution(C, strOutputLayer, DIV8(6 * W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C, In("C", C), p.Activation) +
                                    DepthwiseMixedConvolution(mix, C + 1, In("B", C), 1, 1, channelsplit) +
                                    BatchNormActivation(C + 1, In("DC", C + 1), p.Activation) +
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
                            BatchNormActivation(C, In("A", A), p.Activation) +
                            Convolution(C, In("B", C), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            Activation("GAP", "LogSoftmax") +
                            Cost("ACT", p.Dataset, p.Classes, "CategoricalCrossEntropy", 0.125f);
                    }
                    break;

                case Scripts.resnet:
                    {
                        var bn = p.Bottleneck ? 1ul : 0ul;
                        const Float K = 2.0f;
                        var W = p.Width * 16;
                        var A = 1ul;
                        var C = 5ul;

                        net += Convolution(1, "Input", DIV8(W), 3, 3, 1, 1, 1, 1);

                        if (p.Bottleneck)
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Activation) +
                                Convolution(2, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                BatchNormActivation(2, "C2", p.Activation) +
                                Convolution(3, "B2", DIV8((UInt)(K * W / 4)), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? BatchNormActivationDropout(3, "C3", p.Activation) : BatchNormActivation(3, "C3", p.Activation)) +
                                Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Convolution(5, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Add(1, "C4,C5"));

                            C = 6;
                        }
                        else
                        {
                            blocks.Add(
                                BatchNormActivation(1, "C1", p.Activation) +
                                Convolution(2, "B1", DIV8(W), 3, 3, 1, 1, 1, 1) +
                                (p.Dropout > 0 ? BatchNormActivationDropout(2, "C2", p.Activation) : BatchNormActivation(2, "C2", p.Activation)) +
                                Convolution(3, "B2", DIV8(W), 3, 3, 1, 1, 1, 1) +
                                Convolution(4, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                                Add(1, "C3,C4"));
                        }

                        for (var g = 0ul; g < p.Groups; g++)
                        {
                            if (g > 0)
                            {
                                W *= 2;

                                var strChannelZeroPad = p.ChannelZeroPad ?
                                    AvgPooling(g, In("A", A)) +
                                    "[CZP" + to_string(g) + "]" + nwl + "Type=ChannelZeroPad" + nwl + "Inputs=" + In("P", g) + nwl + "Channels=" + to_string(W) + nwl + nwl +
                                    Add(A + 1, In("C", C + 1 + bn) + "," + In("CZP", g)) :
                                    AvgPooling(g, In("B", C)) +
                                    Convolution(C + 2 + bn, In("P", g), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    Add(A + 1, In("C", C + 1 + bn) + "," + In("C", C + 2 + bn));

                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 2, 2, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 2, In("C", C + 1), p.Activation) : BatchNormActivation(C + 2, In("C", C + 1), p.Activation)) +
                                        Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        strChannelZeroPad);
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(W), 3, 3, 2, 2, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 1, In("C", C), p.Activation) : BatchNormActivation(C + 1, In("C", C), p.Activation)) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        strChannelZeroPad);
                                }

                                A++;
                                C += p.ChannelZeroPad ? 2 + bn : 3 + bn;
                            }

                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                if (p.Bottleneck)
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                        Convolution(C + 1, In("B", C + 1), DIV8((UInt)(K * W / 4)), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 2, In("C", C + 1), p.Activation) : BatchNormActivation(C + 2, In("C", C + 1), p.Activation)) +
                                        Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                        Add(A + 1, In("C", C + 2) + "," + In("A", A)));

                                    C += 3;
                                }
                                else
                                {
                                    blocks.Add(
                                        BatchNormActivation(C, In("A", A), p.Activation) +
                                        Convolution(C, In("B", C), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        (p.Dropout > 0 ? BatchNormActivationDropout(C + 1, In("C", C), p.Activation) : BatchNormActivation(C + 1, In("C", C), p.Activation)) +
                                        Convolution(C + 1, In("B", C + 1), DIV8(W), 3, 3, 1, 1, 1, 1) +
                                        Add(A + 1, In("C", C + 1) + "," + In("A", A)));

                                    C += 2;
                                }
                                A++;
                            }
                        }

                        foreach (var block in blocks)
                            net += block;

                        net +=
                            BatchNormActivation(C, In("A", A), p.Activation) +
                            Convolution(C, In("B", C), p.Classes, 1, 1, 1, 1, 0, 0) +
                            BatchNorm(C + 1, In("C", C)) +
                            GlobalAvgPooling(In("B", C + 1)) +
                            Activation("GAP", "LogSoftmax") +
                            Cost("ACT", p.Dataset, p.Classes, "CategoricalCrossEntropy", 0.125f);
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
                            BatchNormActivation(1, "C1", p.Activation) +
                            Convolution(2, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(2, "C2", p.Activation) +
                            DepthwiseConvolution(3, "B2", 1, kernel, kernel, 1, 1, pad, pad) +
                            BatchNorm(3, "DC3") +
                            Convolution(4, "B3", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            BatchNormActivation(4, "C4", p.Activation) +
                            Convolution(5, "B1", DIV8(W), 1, 1, 1, 1, 0, 0) +
                            Concat(1, "C5,B4"));

                        var C = 6ul;
                        var A = 1ul;

                        for (var g = 1ul; g <= p.Groups; g++)
                        {
                            if (g > 1)
                            {
                                se = p.SqueezeExcitation;
                                W *= 2;

                                blocks.Add(
                                    Convolution(C, In("CC", A), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                    DepthwiseConvolution(C + 1, In("B", C + 1), 1, kernel, kernel, 2, 2, pad, pad) +
                                    BatchNorm(C + 2, In("DC", C + 1)) +
                                    Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 3, In("C", C + 2), p.Activation) +
                                    DepthwiseConvolution(C + 3, In("CC", A), 1, kernel, kernel, 2, 2, pad, pad) +
                                    BatchNorm(C + 4, In("DC", C + 3)) +
                                    Convolution(C + 4, In("B", C + 4), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 5, In("C", C + 4), p.Activation) +
                                    Concat(A + 1, In("B", C + 5) + "," + In("B", C + 3)));

                                A++; C += 5;
                            }

                            for (var i = 1ul; i < p.Iterations; i++)
                            {
                                var group = In("SE", C + 3);
                                var strSE =
                                    se ? GlobalAvgPooling(In("B", C + 3), group) +
                                    Convolution(1, group + "GAP", DIV8(W / 4), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(1, group + "C1", p.Activation == Activations.FRelu ? Activations.HardSwish : p.Activation, group) +
                                    Convolution(2, group + "B1", DIV8(W), 1, 1, 1, 1, 0, 0, group) +
                                    BatchNormActivation(2, group + "C2", "HardLogistic", group) +
                                    ChannelMultiply(In("B", C + 3) + "," + group + "B2", group) +
                                    Concat(A + 1, In("LCS", A) + "," + group + "CM") :
                                    Concat(A + 1, In("LCS", A) + "," + In("B", C + 3));

                                blocks.Add(
                                    ChannelShuffle(A, In("CC", A), 2) +
                                    ChannelSplit(A, In("CSH", A), 2, 1, "L") + ChannelSplit(A, In("CSH", A), 2, 2, "R") +
                                    Convolution(C, In("RCS", A), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 1, In("C", C), p.Activation) +
                                    DepthwiseConvolution(C + 1, In("B", C + 1), 1, kernel, kernel, 1, 1, pad, pad) +
                                    BatchNorm(C + 2, In("DC", C + 1)) +
                                    Convolution(C + 2, In("B", C + 2), DIV8(W), 1, 1, 1, 1, 0, 0) +
                                    BatchNormActivation(C + 3, In("C", C + 2), p.Activation) +
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
                            Activation("GAP", "LogSoftmax") +
                            Cost("ACT", p.Dataset, p.Classes, "CategoricalCrossEntropy", 0.125f);
                    }
                    break;
            }

            return net;
        }
    }
}