using Microsoft.Build.Tasks;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using static NuGet.Client.ManagedCodeConventions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Float = System.Single;
using UInt = System.UInt64;

namespace Convnet.dnncores
{
    public enum TaskStates
    {
        Paused = 0,
		Running = 1,
		Stopped = 2
	};

    public enum States
    {
        Idle = 0,
		NewEpoch = 1,
		Testing = 2,
		Training = 3,
		SaveWeights = 4,
		Completed = 5
	};

    public enum Optimizers
    {
        AdaBound = 0,
		AdaBoundW = 1,
		AdaDelta = 2,
		AdaGrad = 3,
		Adam = 4,
		Adamax = 5,
		AdamW = 6,
		AmsBound = 7,
		AmsBoundW = 8,
		NAG = 9,
		RMSProp = 10,
		SGD = 11,
		SGDMomentum = 12,
		SGDW = 13
	};

    public enum Costs
    {
        BinaryCrossEntropy = 0,
		CategoricalCrossEntropy = 1,
		MeanAbsoluteEpsError = 2,
		MeanAbsoluteError = 3,
		MeanSquaredError = 4,
		SmoothHinge = 5
	};

    public enum Fillers
    {
        Constant = 0,
		HeNormal = 1,
		HeUniform = 2,
		LecunNormal = 3,
		LeCunUniform = 4,
		Normal = 5,
		TruncatedNormal = 6,
		Uniform = 7
	};

    public enum FillerModes
    {
        Avg = 0,
		In = 1,
		Out = 2
	};

    public enum LayerTypes
    {
        Activation = 0,
		Add = 1,
		Average = 2,
		AvgPooling = 3,
		BatchNorm = 4,
		BatchNormActivation = 5,
		BatchNormActivationDropout = 6,
		BatchNormRelu = 7,
		ChannelSplit = 8,
		ChannelSplitRatioLeft = 9,
		ChannelSplitRatioRight = 10,
		ChannelZeroPad = 11,
		Concat = 12,
		Convolution = 13,
		ConvolutionTranspose = 14,
		Cost = 15,
		Dense = 16,
		DepthwiseConvolution = 17,
		Divide = 18,
		Dropout = 19,
		GlobalAvgPooling = 20,
		GlobalMaxPooling = 21,
		GroupNorm = 22,
		Input = 23,
		LayerNorm = 24,
		LocalResponseNorm = 25,
		LogSoftmax = 26,
		Max = 27,
		MaxPooling = 28,
		Min = 29,
		Multiply = 30,
		PartialDepthwiseConvolution = 31,
		PRelu = 32,
		Reduction = 33,
		Resampling = 34,
		Shuffle = 35,
		Softmax = 36,
		Substract = 37
	};

    public enum Activations
    {
        Abs = 0,
		ASinh = 1,
		BoundedRelu = 2,
		Clip = 3,
		ClipV2 = 4,			//
		Elu = 5,			//
		Exp = 6,			//
		GeluErf = 7,
		GeluTanh = 8,
		HardSigmoid = 9,
		HardSwish = 10,
		Linear = 11,
		Log = 12,
		LogSigmoid = 13,
		Mish = 14,
		Pow = 15,
		Relu = 16,			//
		Round = 17,
		Selu = 18,
		Sigmoid = 19,		//
		SoftPlus = 20,
		SoftRelu = 21,
		SoftSign = 22,
		Sqrt = 23,			//
		Square = 24,
		Swish = 25,
		Tanh = 26,			//
		TanhExp = 27
	};

    public enum Algorithms
    {
        Linear = 0,
		Nearest = 1
	};

    public enum Datasets
    {
        cifar10 = 0,
		cifar100 = 1,
		fashionmnist = 2,
		mnist = 3,
		tinyimagenet = 4
	};

    public enum Models
    {
        densenet = 0,
		efficientnetv2 = 1,
		mobilenetv3 = 2,
		resnet = 3,
		shufflenetv2 = 4
	};

    public enum Positions
    {
        TopLeft = 0,
		TopRight = 1,
		BottomLeft = 2,
		BottomRight = 3,
		Center = 4
	};

    public enum Interpolations
    {
        Cubic = 0,
		Linear = 1,
		Nearest = 2
	};

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct Stats
    {
        public Float Mean;
        public Float StdDev;
        public Float Min;
        public Float Max;

        public Stats()
        {
            Mean = (Float)0;
            StdDev = (Float)0;
            Min = (Float)0;
            Max = (Float)0;
        }

        public Stats(Float mean, Float stddev, Float min, Float max)
        {
            Mean = mean;
			StdDev = stddev;
            Min = min;
            Max = max;
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct CheckMsg
    {
        public UInt Row;
        public UInt Column;
        public bool Error;
        [MarshalAs(UnmanagedType.LPStr)] // Unicode: LPWStr
        public string Message;

        public CheckMsg(UInt row = 0, UInt column = 0, string message = "", bool error = true)
        {
            Row = row;
            Column = column;
            Message = message;
            Error = error;
        }
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ModelInfo
    {
        [MarshalAs(UnmanagedType.LPStr)] // Unicode: LPWStr
        public string Name;
        public Datasets Dataset;
        public Costs CostFunction;
        public UInt LayerCount;
        public UInt CostLayerCount;
        public UInt CostIndex;
        public UInt GroupIndex;
        public UInt LabelIndex;
        public UInt Hierarchies;
        public UInt TrainSamplesCount;
        public UInt TestSamplesCount;
        public bool MeanStdNormalization;
        public Float[] MeanTrainSet;
        public Float[] StdTrainSet;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TrainingInfo
    {
        public UInt TotalCycles;
        public UInt TotalEpochs;
        public UInt Cycle;
        public UInt Epoch;
        public UInt SampleIndex;
        public Float Rate;
        public Optimizers Optimizer;
        public Float Momentum;
        public Float Beta2;
        public Float Gamma;
        public Float L2Penalty;
        public Float Dropout;
        public UInt BatchSize;
        public UInt Height;
        public UInt Width;
        public UInt PadH;
        public UInt PadW;
        public bool HorizontalFlip;
        public bool VerticalFlip;
        public Float InputDropout;
        public Float Cutout;
        public bool CutMix;
        public Float AutoAugment;
        public Float ColorCast;
        public UInt ColorAngle;
        public Float Distortion;
        public Interpolations Interpolation;
        public Float Scaling;
        public Float Rotation;
        public Float AvgTrainLoss;
        public Float TrainErrorPercentage;
        public UInt TrainErrors;
        public Float AvgTestLoss;
        public Float TestErrorPercentage;
        public UInt TestErrors;
        public Float SampleSpeed;
        public States State;
        public TaskStates TaskState;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TestingInfo
    {
        public UInt TotalCycles;
        public UInt TotalEpochs;
        public UInt Cycle;
        public UInt Epoch;
        public UInt SampleIndex;
        public UInt BatchSize;
        public UInt Height;
        public UInt Width;
        public UInt PadH;
        public UInt PadW;
        public Float AvgTestLoss;
        public Float TestErrorPercentage;
        public UInt TestErrors;
        public Float SampleSpeed;
        public States State;
        public TaskStates TaskState;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct LayerInfo
    {
        public string Name;
        public string Description;
        public LayerTypes LayerType;
        public Activations Activation;
        public Algorithms Algorithm;
        public Costs Cost;
        public UInt NeuronCount;
        public UInt WeightCount;
        public UInt BiasesCount;
        public UInt LayerIndex;
        public UInt InputsCount;
        public UInt C;
        public UInt D;
        public UInt H;
        public UInt W;
        public UInt PadD;
        public UInt PadH;
        public UInt PadW;
        public UInt KernelH;
        public UInt KernelW;
        public UInt StrideH;
        public UInt StrideW;
        public UInt DilationH;
        public UInt DilationW;
        public UInt Multiplier;
        public UInt Groups;
        public UInt Group;
        public UInt LocalSize;
        public Float Dropout;
        public Float LabelTrue;
        public Float LabelFalse;
        public Float Weight;
        public UInt GroupIndex;
        public UInt LabelIndex;
        public UInt InputC;
        public Float Alpha;
        public Float Beta;
        public Float K;
        public Float fH;
        public Float fW;
        public bool HasBias;
        public bool Scaling;
        public bool AcrossChannels;
        public bool Locked;
        public bool Lockable;
    };

    [Serializable()]
    public enum DNNAlgorithms
    {
        Linear = 0,
        Nearest = 1
    };

    [Serializable()]
    public enum DNNInterpolations
    {
        Cubic = 0,
        Linear = 1,
        Nearest = 2
    };

    [Serializable()]
    public enum DNNOptimizers
    {
        AdaBound = 0,
        AdaBoundW = 1,
        AdaDelta = 2,
        AdaGrad = 3,
        Adam = 4,
        Adamax = 5,
        AdamW = 6,
        AmsBound = 7,
        AmsBoundW = 8,
        NAG = 9,
        RMSProp = 10,
        SGD = 11,
        SGDMomentum = 12,
        SGDW = 13
    };

    [Serializable()]
    public enum DNNDatasets
    {
        cifar10 = 0,
        cifar100 = 1,
        fashionmnist = 2,
        mnist = 3,
        tinyimagenet = 4
    };

    [Serializable()]
    public enum DNNScripts
    {
        densenet = 0,
        efficientnetv2 = 1,
        mobilenetv3 = 2,
        resnet = 3,
        shufflenetv2 = 4
    };

    [Serializable()]
    public enum DNNCosts
    {
        BinaryCrossEntropy = 0,
        CategoricalCrossEntropy = 1,
        MeanAbsoluteEpsError = 2,
        MeanAbsoluteError = 3,
        MeanSquaredError = 4,
        SmoothHinge = 5
    };

    [Serializable()]
    public enum DNNFillers
    {
        Constant = 0,
        HeNormal = 1,
        HeUniform = 2,
        LeCunNormal = 3,
        LeCunUniform = 4,
        Normal = 5,
        TruncatedNormal = 6,
        Uniform = 7
    };

    [Serializable()]
    public enum DNNFillerModes
    {
        Avg = 0,
        In = 1,
        Out = 2
    };

    [Serializable()]
    public enum DNNLayerTypes
    {
        Activation = 0,
        Add = 1,
        Average = 2,
        AvgPooling = 3,
        BatchNorm = 4,
        BatchNormActivation = 5,
        BatchNormActivationDropout = 6,
        BatchNormRelu = 7,
        ChannelSplit = 8,
        ChannelSplitRatioLeft = 9,
        ChannelSplitRatioRight = 10,
        ChannelZeroPad = 11,
        Concat = 12,
        Convolution = 13,
        ConvolutionTranspose = 14,
        Cost = 15,
        Dense = 16,
        DepthwiseConvolution = 17,
        Divide = 18,
        Dropout = 19,
        GlobalAvgPooling = 20,
        GlobalMaxPooling = 21,
        GroupNorm = 22,
        Input = 23,
        LayerNorm = 24,
        LocalResponseNorm = 25,
        LogSoftmax = 26,
        Max = 27,
        MaxPooling = 28,
        Min = 29,
        Multiply = 30,
        PartialDepthwiseConvolution = 31,
        PRelu = 32,
        Reduction = 33,
        Resampling = 34,
        Shuffle = 35,
        Softmax = 36,
        Substract = 37
    };

    [Serializable()]
    public enum DNNActivations
    {
        Abs = 0,
        ASinh = 1,
        BoundedRelu = 2,
        Clip = 3,
        ClipV2 = 4,         //
        Elu = 5,            //
        Exp = 6,            //
        GeluErf = 7,
        GeluTanh = 8,
        HardSigmoid = 9,
        HardSwish = 10,
        Linear = 11,
        Log = 12,
        LogSigmoid = 13,
        Mish = 14,
        Pow = 15,
        Relu = 16,          //
        Round = 17,
        Selu = 18,
        Sigmoid = 19,       //
        SoftPlus = 20,
        SoftRelu = 21,
        SoftSign = 22,
        Sqrt = 23,          //
        Square = 24,
        Swish = 25,
        Tanh = 26,          //
        TanhExp = 27
    };

    [Serializable()]
    public enum DNNStates
    {
        Idle = 0,
        NewEpoch = 1,
        Testing = 2,
        Training = 3,
        SaveWeights = 4,
        Completed = 5
    };

    [Serializable()]
    public enum DNNTaskStates
    {
        Paused = 0,
        Running = 1,
        Stopped = 2
    };

    [Serializable()]
    public class DNNStats
    {
        public Float Mean;
        public Float StdDev;
        public Float Min;
        public Float Max;

        //DNNStats(dnn::Stats& stats)
        //{
        // Mean = stats.Mean;
        // StdDev = stats.StdDev;
        // Min = stats.Min;
        // Max = stats.Max;
        //}

        public DNNStats(Float mean, Float stddev, Float min, Float max)
        {
            Mean = mean;
            StdDev = stddev;
            Min = min;
            Max = max;
        }
    };

    [Serializable()]
    public class DNNCostLayer
    {
        public DNNCosts CostFunction;
        public UInt LayerIndex;
        public UInt GroupIndex;
        public UInt LabelIndex;
        public UInt ClassCount;
        public string Name;
        public Float Weight;
        public UInt TrainErrors;
        public Float TrainLoss;
        public Float AvgTrainLoss;
        public Float TrainErrorPercentage;
        public Float TrainAccuracy;
        public UInt TestErrors;
        public Float TestLoss;
        public Float AvgTestLoss;
        public Float TestErrorPercentage;
        public Float TestAccuracy;

        public DNNCostLayer(DNNCosts costFunction, UInt layerIndex, UInt groupIndex, UInt labelIndex, UInt classCount, string name, Float weight)
        {
            CostFunction = costFunction;
            LayerIndex = layerIndex;
            GroupIndex = groupIndex;
            LabelIndex = labelIndex;
            ClassCount = classCount;
            Name = name;
            Weight = weight;

            TrainErrors = 0;
            TrainLoss = (Float)0;
            AvgTrainLoss = (Float)0;
            TrainErrorPercentage = (Float)0;
            TrainAccuracy = (Float)0;

            TestErrors = 0;
            TestLoss = (Float)0;
            AvgTestLoss = (Float)0;
            TestErrorPercentage = (Float)0;
            TestAccuracy = (Float)0;
        }
    };

    [Serializable()]
    public class DNNTrainingRate : System.ComponentModel.INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private DNNOptimizers optimizer = DNNOptimizers.NAG;
        private Float momentum = (Float)0.9;
        private Float beta2 = (Float)0.999;
        private Float l2Penalty = (Float)0.0005;
        private Float dropout = (Float)0;
        private Float eps = (Float)1E-08;
        private UInt n = 128;
        private UInt d = 1;
        private UInt h = 32;
        private UInt w = 32;
        private UInt padD = 0;
        private UInt padH = 4;
        private UInt padW = 4;
        private UInt cycles = 1;
        private UInt epochs = 200;
        private UInt epochMultiplier = 1;
        private Float maximumRate = (Float)0.05;
        private Float minimumRate = (Float)0.0001;
        private Float finalRate = (Float)0.1;
        private Float gamma = (Float)0.003;
        private UInt decayAfterEpochs = 1;
        private Float decayFactor = (Float)1;
        private bool horizontalFlip = false;
        private bool verticalFlip = false;
        private Float inputDropout = (Float)0;
        private Float cutout = (Float)0;
        private bool cutMix = false;
        private Float autoAugment = (Float)0;
        private Float colorCast = (Float)0;
        private UInt colorAngle = 0;
        private Float distortion = (Float)0;
        private DNNInterpolations interpolation = DNNInterpolations.Linear;
        private Float scaling = (Float)10;
        private Float rotation = (Float)12;

        public DNNOptimizers Optimizer
        {
            get { return optimizer; }
            set
            {
                if (value == optimizer)
                    return;

                optimizer = value;
                OnPropertyChanged(nameof(Optimizer));
            }
        }
        public Float Momentum
        {
            get { return momentum; }
            set
            {
                if (value == momentum || value < (Float)0 || value > (Float)1)
                    return;

                momentum = value;
                OnPropertyChanged(nameof(Momentum));
            }
        }
        public Float Beta2
        {
            get { return beta2; }
            set
            {
                if (value == beta2 || value < (Float)0 || value > (Float)1)
                    return;

                beta2 = value;
                OnPropertyChanged(nameof(Beta2));
            }
        }
        public Float L2Penalty
        {
            get { return l2Penalty; }
            set
            {
                if (value == l2Penalty || value < (Float)0 || value > (Float)1)
                    return;

                l2Penalty = value;
                OnPropertyChanged(nameof(L2Penalty));
            }
        }
        public Float Dropout
        {
            get { return dropout; }
            set
            {
                if (value == dropout || value < (Float)0 || value > (Float)1)
                    return;

                dropout = value;
                OnPropertyChanged(nameof(Dropout));
            }
        }
        public Float Eps
        {
            get { return eps; }
            set
            {
                if (value == eps || value < (Float)0 || value > (Float)1)
                    return;

                eps = value;
                OnPropertyChanged(nameof(Eps));
            }
        }
        public UInt N
        {
            get { return n; }
            set
            {
                if (value == n && value == 0)
                    return;

                n = value;
                OnPropertyChanged(nameof(N));
            }
        }
        public UInt D
        {
            get { return d; }
            set
            {
                if (value == d && value == 0)
                    return;

                d = value;
                OnPropertyChanged(nameof(D));
            }
        }
        public UInt H
        {
            get { return h; }
            set
            {
                if (value == h && value == 0)
                    return;

                h = value;
                OnPropertyChanged(nameof(H));
            }
        }
        public UInt W
        {
            get { return w; }
            set
            {
                if (value == w && value == 0)
                    return;

                w = value;
                OnPropertyChanged(nameof(W));
            }
        }
        public UInt PadD
        {
            get { return padD; }
            set
            {
                if (value == padD && value == 0)
                    return;

                padD = value;
                OnPropertyChanged(nameof(PadD));
            }
        }
        public UInt PadH
        {
            get { return padH; }
            set
            {
                if (value == padH && value == 0)
                    return;

                padH = value;
                OnPropertyChanged(nameof(PadH));
            }
        }
        public UInt PadW
        {
            get { return padW; }
            set
            {
                if (value == padW && value == 0)
                    return;

                padW = value;
                OnPropertyChanged(nameof(PadW));
            }
        }
        public UInt Cycles
        {
            get { return cycles; }
            set
            {
                if (value == cycles && value == 0)
                    return;

                cycles = value;
                OnPropertyChanged(nameof(Cycles));
            }
        }
        public UInt Epochs
        {
            get { return epochs; }
            set
            {
                if (value == epochs && value == 0)
                    return;

                epochs = value;
                OnPropertyChanged(nameof(Epochs));
            }
        }
        public UInt EpochMultiplier
        {
            get { return epochMultiplier; }
            set
            {
                if (value == epochMultiplier && value == 0)
                    return;

                epochMultiplier = value;
                OnPropertyChanged(nameof(EpochMultiplier));
            }
        }
        public Float MaximumRate
        {
            get { return maximumRate; }
            set
            {
                if (value == maximumRate || value < (Float)0 || value > (Float)1)
                    return;

                maximumRate = value;
                OnPropertyChanged(nameof(MaximumRate));
            }
        }
        public Float MinimumRate
        {
            get { return minimumRate; }
            set
            {
                if (value == minimumRate || value < (Float)0 || value > (Float)1)
                    return;

                minimumRate = value;
                OnPropertyChanged(nameof(MinimumRate));
            }
        }
        public Float FinalRate
        {
            get { return finalRate; }
            set
            {
                if (value == finalRate || value < (Float)0 || value > (Float)1)
                    return;

                finalRate = value;
                OnPropertyChanged(nameof(FinalRate));
            }
        }
        public Float Gamma
        {
            get { return gamma; }
            set
            {
                if (value == gamma || value < (Float)0 || value > (Float)1)
                    return;

                gamma = value;
                OnPropertyChanged(nameof(Gamma));
            }
        }
        public UInt DecayAfterEpochs
        {
            get { return decayAfterEpochs; }
            set
            {
                if (value == decayAfterEpochs && value == 0)
                    return;

                decayAfterEpochs = value;
                OnPropertyChanged(nameof(DecayAfterEpochs));
            }
        }
        public Float DecayFactor
        {
            get { return decayFactor; }
            set
            {
                if (value == decayFactor || value < (Float)0 || value > (Float)1)
                    return;

                decayFactor = value;
                OnPropertyChanged(nameof(DecayFactor));
            }
        }
        public bool HorizontalFlip
        {
            get { return horizontalFlip; }
            set
            {
                if (value == horizontalFlip)
                    return;

                horizontalFlip = value;
                OnPropertyChanged(nameof(HorizontalFlip));
            }
        }
        public bool VerticalFlip
        {
            get { return verticalFlip; }
            set
            {
                if (value == verticalFlip)
                    return;

                verticalFlip = value;
                OnPropertyChanged(nameof(VerticalFlip));
            }
        }
        public Float InputDropout
        {
            get { return inputDropout; }
            set
            {
                if (value == inputDropout || value < (Float)0 || value > (Float)1)
                    return;

                inputDropout = value;
                OnPropertyChanged(nameof(InputDropout));
            }
        }
        public Float Cutout
        {
            get { return cutout; }
            set
            {
                if (value == cutout || value < (Float)0 || value > (Float)1)
                    return;

                cutout = value;
                OnPropertyChanged(nameof(Cutout));
            }
        }
        public bool CutMix
        {
            get { return cutMix; }
            set
            {
                if (value == cutMix)
                    return;

                cutMix = value;
                OnPropertyChanged(nameof(CutMix));
            }
        }
        public Float AutoAugment
        {
            get { return autoAugment; }
            set
            {
                if (value == autoAugment || value < (Float)0 || value > (Float)1)
                    return;

                autoAugment = value;
                OnPropertyChanged(nameof(AutoAugment));
            }
        }
        public Float ColorCast
        {
            get { return colorCast; }
            set
            {
                if (value == colorCast || value < (Float)0 || value > (Float)1)
                    return;

                colorCast = value;
                OnPropertyChanged(nameof(ColorCast));
            }
        }
        public UInt ColorAngle
        {
            get { return colorAngle; }
            set
            {
                if (value == colorAngle || value > (Float)360)
                    return;

                colorAngle = value;
                OnPropertyChanged(nameof(ColorAngle));
            }
        }
        public Float Distortion
        {
            get { return distortion; }
            set
            {
                if (value == distortion || value < (Float)0 || value > (Float)1)
                    return;

                distortion = value;
                OnPropertyChanged(nameof(Distortion));
            }
        }
        public DNNInterpolations Interpolation
        {
            get { return interpolation; }
            set
            {
                if (value == interpolation)
                    return;

                interpolation = value;
                OnPropertyChanged(nameof(Interpolation));
            }
        }
        public Float Scaling
        {
            get { return scaling; }
            set
            {
                if (value == scaling || value <= (Float)0 || value > (Float)200)
                    return;

                scaling = value;
                OnPropertyChanged(nameof(Scaling));
            }
        }
        public Float Rotation
        {
            get { return rotation; }
            set
            {
                if (value == rotation || value < (Float)0 || value > (Float)360)
                    return;

                rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }

        public DNNTrainingRate()
        {
            optimizer = DNNOptimizers.NAG;
            momentum = (Float)0.9;
            beta2 = (Float)0.999;
            l2Penalty = (Float)0.0005;
            dropout = 0;
            eps = (Float)1E-08;
            n = 128;
            d = 1;
            h = 32;
            w = 32;
            padD = 0;
            padH = 4;
            padW = 4;
            cycles = 1;
            epochs = 200;
            epochMultiplier = 1;
            maximumRate = (Float)0.05;
            minimumRate = (Float)0.0001;
            finalRate = (Float)0.1;
            gamma = (Float)0.003;
            decayAfterEpochs = 1;
            decayFactor = 1;
            horizontalFlip = true;
            verticalFlip = false;
            inputDropout = 0;
            cutout = 0;
            cutMix = false;
            autoAugment = 0;
            colorCast = 0;
            colorAngle = 16;
            distortion = 0;
            interpolation = DNNInterpolations.Linear;
            scaling = 10;
            rotation = 12;
        }

        public DNNTrainingRate(DNNOptimizers optimizer, Float momentum, Float beta2, Float l2penalty, Float dropout, Float eps, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, UInt cycles, UInt epochs, UInt epochMultiplier, Float maximumRate, Float minimumRate, Float finalRate, Float gamma, UInt decayAfterEpochs, Float decayFactor, bool horizontalFlip, bool verticalFlip, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation)
        {
            Optimizer = optimizer;
            Momentum = momentum;
            Beta2 = beta2;
            L2Penalty = l2penalty;
            Dropout = dropout;
            Eps = eps;
            N = n;
            D = d;
            H = h;
            W = w;
            PadD = padD;
            PadH = padH;
            PadW = padW;
            Cycles = cycles;
            Epochs = epochs;
            EpochMultiplier = epochMultiplier;
            MaximumRate = maximumRate;
            MinimumRate = minimumRate;
            FinalRate = finalRate;
            Gamma = gamma;
            DecayAfterEpochs = decayAfterEpochs;
            DecayFactor = decayFactor;
            HorizontalFlip = horizontalFlip;
            VerticalFlip = verticalFlip;
            InputDropout = inputDropout;
            Cutout = cutout;
            CutMix = cutMix;
            AutoAugment = autoAugment;
            ColorCast = colorCast;
            ColorAngle = colorAngle;
            Distortion = distortion;
            Interpolation = interpolation;
            Scaling = scaling;
            Rotation = rotation;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    };

    [Serializable()]
    public class DNNTrainingStrategy : System.ComponentModel.INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private Float epochs = (Float)1;
        private UInt n = 128;
        private UInt d = 1;
        private UInt h = 32;
        private UInt w = 32;
        private UInt padD = 0;
        private UInt padH = 4;
        private UInt padW = 4;
        private Float momentum = (Float)0.9;
        private Float beta2 = (Float)0.999;
        private Float gamma = (Float)0.003;
        private Float l2Penalty = (Float)0.0005;
        private Float dropout = (Float)0;
        private bool horizontalFlip = false;
        private bool verticalFlip = false;
        private Float inputDropout = (Float)0;
        private Float cutout = (Float)0;
        private bool cutMix = false;
        private Float autoAugment = (Float)0;
        private Float colorCast = (Float)0;
        private UInt colorAngle = 0;
        private Float distortion = (Float)0;
        private DNNInterpolations interpolation = DNNInterpolations.Linear;
        private Float scaling = (Float)10;
        private Float rotation = (Float)12;

        public Float Epochs
        {
            get { return epochs; }
            set
            {
                if (value == epochs || value <= 0 || value > 1)
                    return;

                epochs = value;
                OnPropertyChanged(nameof(Epochs));
            }
        }
        public UInt N
        {
            get { return n; }
            set
            {
                if (value == n && value == 0)
                    return;

                n = value;
                OnPropertyChanged(nameof(N));
            }
        }
        public UInt D
        {
            get { return d; }
            set
            {
                if (value == d && value == 0)
                    return;

                d = value;
                OnPropertyChanged(nameof(D));
            }
        }
        public UInt H
        {
            get { return h; }
            set
            {
                if (value == h && value == 0)
                    return;

                h = value;
                OnPropertyChanged(nameof(H));
            }
        }
        public UInt W
        {
            get { return w; }
            set
            {
                if (value == w && value == 0)
                    return;

                w = value;
                OnPropertyChanged(nameof(W));
            }
        }
        public UInt PadD
        {
            get { return padD; }
            set
            {
                if (value == padD && value == 0)
                    return;

                padD = value;
                OnPropertyChanged(nameof(PadD));
            }
        }
        public UInt PadH
        {
            get { return padH; }
            set
            {
                if (value == padH && value == 0)
                    return;

                padH = value;
                OnPropertyChanged(nameof(PadH));
            }
        }
        public UInt PadW
        {
            get { return padW; }
            set
            {
                if (value == padW && value == 0)
                    return;

                padW = value;
                OnPropertyChanged(nameof(PadW));
            }
        }
        public Float Momentum
        {
            get { return momentum; }
            set
            {
                if (value == momentum || value < (Float)0 || value > (Float)1)
                    return;

                momentum = value;
                OnPropertyChanged(nameof(Momentum));
            }
        }
        public Float Beta2
        {
            get { return beta2; }
            set
            {
                if (value == beta2 || value < (Float)0 || value > (Float)1)
                    return;

                beta2 = value;
                OnPropertyChanged(nameof(Beta2));
            }
        }
        public Float Gamma
        {
            get { return gamma; }
            set
            {
                if (value == gamma || value < (Float)0 || value > (Float)1)
                    return;

                gamma = value;
                OnPropertyChanged(nameof(Gamma));
            }
        }
        public Float L2Penalty
        {
            get { return l2Penalty; }
            set
            {
                if (value == l2Penalty || value < (Float)0 || value > (Float)1)
                    return;

                l2Penalty = value;
                OnPropertyChanged(nameof(L2Penalty));
            }
        }
        public Float Dropout
        {
            get { return dropout; }
            set
            {
                if (value == dropout || value < (Float)0 || value > (Float)1)
                    return;

                dropout = value;
                OnPropertyChanged(nameof(Dropout));
            }
        }
        public bool HorizontalFlip
        {
            get { return horizontalFlip; }
            set
            {
                if (value == horizontalFlip)
                    return;

                horizontalFlip = value;
                OnPropertyChanged(nameof(HorizontalFlip));
            }
        }
        public bool VerticalFlip
        {
            get { return verticalFlip; }
            set
            {
                if (value == verticalFlip)
                    return;

                verticalFlip = value;
                OnPropertyChanged(nameof(VerticalFlip));
            }
        }
        public Float InputDropout
        {
            get { return inputDropout; }
            set
            {
                if (value == inputDropout || value < (Float)0 || value > (Float)1)
                    return;

                inputDropout = value;
                OnPropertyChanged(nameof(InputDropout));
            }
        }
        public Float Cutout
        {
            get { return cutout; }
            set
            {
                if (value == cutout || value < (Float)0 || value > (Float)1)
                    return;

                cutout = value;
                OnPropertyChanged(nameof(Cutout));
            }
        }
        public bool CutMix
        {
            get { return cutMix; }
            set
            {
                if (value == cutMix)
                    return;

                cutMix = value;
                OnPropertyChanged(nameof(CutMix));
            }
        }
        public Float AutoAugment
        {
            get { return autoAugment; }
            set
            {
                if (value == autoAugment || value < (Float)0 || value > (Float)1)
                    return;

                autoAugment = value;
                OnPropertyChanged(nameof(AutoAugment));
            }
        }
        public Float ColorCast
        {
            get { return colorCast; }
            set
            {
                if (value == colorCast || value < (Float)0 || value > (Float)1)
                    return;

                colorCast = value;
                OnPropertyChanged(nameof(ColorCast));
            }
        }
        public UInt ColorAngle
        {
            get { return colorAngle; }
            set
            {
                if (value == colorAngle || value < (Float)0 || value > (Float)360)
                    return;

                colorAngle = value;
                OnPropertyChanged(nameof(ColorAngle));
            }
        }
        public Float Distortion
        {
            get { return distortion; }
            set
            {
                if (value == distortion || value < (Float)0 || value > (Float)1)
                    return;

                distortion = value;
                OnPropertyChanged(nameof(Distortion));
            }
        }
        public DNNInterpolations Interpolation
        {
            get { return interpolation; }
            set
            {
                if (value == interpolation)
                    return;

                interpolation = value;
                OnPropertyChanged(nameof(Interpolation));

            }
        }
        public Float Scaling
        {
            get { return scaling; }
            set
            {
                if (value == scaling || value <= (Float)0 || value > (Float)200)
                    return;

                scaling = value;
                OnPropertyChanged(nameof(Scaling));
            }
        }
        public Float Rotation

        {
            get { return rotation; }
            set
            {
                if (value == rotation || value < (Float)0 || value > (Float)360)
                    return;

                rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }

        public DNNTrainingStrategy()
        {
            epochs = (Float)1;
            n = 128;
            d = 1;
            h = 32;
            w = 32;
            padD = 0;
            padH = 4;
            padW = 4;
            momentum = (Float)0.9;
            beta2 = (Float)0.999;
            gamma = (Float)0.003;
            l2Penalty = (Float)0.0005;
            dropout = (Float)0;
            horizontalFlip = true;
            verticalFlip = false;
            inputDropout = (Float)0;
            cutout = (Float)0;
            cutMix = false;
            autoAugment = (Float)0;
            colorCast = (Float)0;
            colorAngle = 16;
            distortion = (Float)0;
            interpolation = DNNInterpolations.Linear;
            scaling = (Float)10;
            rotation = (Float)12;
        }

        public DNNTrainingStrategy(Float epochs, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, Float momentum, Float beta2, Float gamma, Float l2penalty, Float dropout, bool horizontalFlip, bool verticalFlip, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation)
        {
            Epochs = epochs;
            N = n;
            D = d;
            H = h;
            W = w;
            PadD = padD;
            PadH = padH;
            PadW = padW;
            Momentum = momentum;
            Beta2 = beta2;
            Gamma = gamma;
            L2Penalty = l2penalty;
            Dropout = dropout;
            HorizontalFlip = horizontalFlip;
            VerticalFlip = verticalFlip;
            InputDropout = inputDropout;
            Cutout = cutout;
            CutMix = cutMix;
            AutoAugment = autoAugment;
            ColorCast = colorCast;
            ColorAngle = colorAngle;
            Distortion = distortion;
            Interpolation = interpolation;
            Scaling = scaling;
            Rotation = rotation;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    };

    [Serializable()]
    public struct DNNTrainingResult
    {
        UInt Cycle;
        UInt Epoch;
        UInt GroupIndex;
        UInt CostIndex;
        string CostName;
        UInt N;
        UInt D;
        UInt H;
        UInt W;
        UInt PadD;
        UInt PadH;
        UInt PadW;
        DNNOptimizers Optimizer;
        Float Rate;
        Float Eps;
        Float Momentum;
        Float Beta2;
        Float Gamma;
        Float L2Penalty;
        Float Dropout;
        Float InputDropout;
        Float Cutout;
        bool CutMix;
        Float AutoAugment;
        bool HorizontalFlip;
        bool VerticalFlip;
        Float ColorCast;
        UInt ColorAngle;
        Float Distortion;
        DNNInterpolations Interpolation;
        Float Scaling;
        Float Rotation;
        Float AvgTrainLoss;
        UInt TrainErrors;
        Float TrainErrorPercentage;
        Float TrainAccuracy;
        Float AvgTestLoss;
        UInt TestErrors;
        Float TestErrorPercentage;
        Float TestAccuracy;
        long ElapsedMilliSeconds;
        TimeSpan ElapsedTime;

        DNNTrainingResult(UInt cycle, UInt epoch, UInt groupIndex, UInt costIndex, string costName, UInt n, UInt d, UInt h, UInt w, UInt padD, UInt padH, UInt padW, DNNOptimizers optimizer, Float rate, Float eps, Float momentum, Float beta2, Float gamma, Float l2Penalty, Float dropout, Float inputDropout, Float cutout, bool cutMix, Float autoAugment, bool horizontalFlip, bool verticalFlip, Float colorCast, UInt colorAngle, Float distortion, DNNInterpolations interpolation, Float scaling, Float rotation, Float avgTrainLoss, UInt trainErrors, Float trainErrorPercentage, Float trainAccuracy, Float avgTestLoss, UInt testErrors, Float testErrorPercentage, Float testAccuracy, long elapsedMilliSeconds, TimeSpan elapsedTime)
        {
            Cycle = cycle;
            Epoch = epoch;
            GroupIndex = groupIndex;
            CostIndex = costIndex;
            CostName = costName;
            N = n;
            D = d;
            H = h;
            W = w;
            PadD = padD;
            PadH = padH;
            PadW = padW;
            Optimizer = optimizer;
            Rate = rate;
            Eps = eps;
            Momentum = momentum;
            Beta2 = beta2;
            Gamma = gamma;
            L2Penalty = l2Penalty;
            Dropout = dropout;
            InputDropout = inputDropout;
            Cutout = cutout;
            CutMix = cutMix;
            AutoAugment = autoAugment;
            HorizontalFlip = horizontalFlip;
            VerticalFlip = verticalFlip;
            ColorCast = colorCast;
            ColorAngle = colorAngle;
            Distortion = distortion;
            Interpolation = interpolation;
            Scaling = scaling;
            Rotation = rotation;
            AvgTrainLoss = avgTrainLoss;
            TrainErrors = trainErrors;
            TrainErrorPercentage = trainErrorPercentage;
            TrainAccuracy = trainAccuracy;
            AvgTestLoss = avgTestLoss;
            TestErrors = testErrors;
            TestErrorPercentage = testErrorPercentage;
            TestAccuracy = testAccuracy;
            ElapsedMilliSeconds = elapsedMilliSeconds;
            ElapsedTime = elapsedTime;
        }
    };

    [Serializable()]
    public struct DNNCheckMsg
    {
        UInt Row;
        UInt Column;
        bool Error;
        string Message;
        string Definition;

        DNNCheckMsg(UInt row, UInt column, string message, bool error, string definition)
        {
            Row = row;
            Column = column;
            Message = message;
            Error = error;
            Definition = definition;
        }
    };

    [Serializable()]
    public class DNNLayerInfo : System.ComponentModel.INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        private Nullable<bool> lockUpdate;

        public string Name;
        public string Description;
        public DNNLayerTypes LayerType;
        public DNNActivations Activation;
        public DNNCosts Cost;
        public System.Collections.Generic.List<UInt> Inputs;
        public System.Collections.Generic.List<string> InputsNames;
        public System.Windows.Media.Imaging.BitmapSource WeightsSnapshot;
        public bool Lockable;
        public Nullable<bool> LockUpdate
        {
            get { return lockUpdate; }
            set
            {
                if (value.Equals(lockUpdate))
                    return;

                lockUpdate = value;
                OnPropertyChanged(nameof(LockUpdate));
            }
        }
        public bool IsNormLayer;
        public bool HasBias;
        public bool MirrorPad;
        public bool RandomCrop;
        public bool Scaling;
        public bool AcrossChannels;
        public int WeightsSnapshotX;
        public int WeightsSnapshotY;
        public UInt InputCount;
        public UInt LayerIndex;
        public UInt NeuronCount;
        public UInt C;
        public UInt D;
        public UInt W;
        public UInt H;
        public UInt KernelH;
        public UInt KernelW;
        public UInt DilationH;
        public UInt DilationW;
        public UInt StrideH;
        public UInt StrideW;
        public UInt PadD;
        public UInt PadH;
        public UInt PadW;
        public UInt Multiplier;
        public UInt Groups;
        public UInt Group;
        public UInt LocalSize;
        public UInt WeightCount;
        public UInt BiasCount;
        public UInt GroupSize;
        public UInt InputC;
        public UInt GroupIndex;
        public UInt LabelIndex;
        public Float Dropout;
        public Float Cutout;
        public DNNStats NeuronsStats;
        public DNNStats WeightsStats;
        public DNNStats BiasesStats;
        public Float Weight;
        public Float Alpha;
        public Float Beta;
        public Float K;
        public DNNAlgorithms Algorithm;
        public Float FactorH;
        public Float FactorW;
        public Float FPropLayerTime;
        public Float BPropLayerTime;
        public Float UpdateLayerTime;

        public DNNLayerInfo()
        {
            NeuronsStats = new DNNStats((Float)0, (Float)0, (Float)0, (Float)0);
            WeightsStats = new DNNStats((Float)0, (Float)0, (Float)0, (Float)0);
            BiasesStats = new DNNStats((Float)0, (Float)0, (Float)0, (Float)0);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class DNNModel
    {
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNDataprovider(string directory);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int DNNRead(string definition, out CheckMsg checkMsg);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern bool DNNLoadDataset();
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNGetTrainingInfo(out TrainingInfo info);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNGetTestingInfo(out TestingInfo info);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNGetModelInfo(out ModelInfo info);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNGetLayerInfo(UInt layerIndex, out LayerInfo info);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern void DNNGetLayerInputs(UInt layerIndex, out UInt[] inputs);
        [DllImport("dnn.dll", EntryPoint = "DllMain", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern Optimizers GetOptimizer();


        static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public delegate void TrainProgressEventDelegate(DNNOptimizers Optim, UInt BatchSize, UInt Cycle, UInt TotalCycles, UInt Epoch, UInt TotalEpochs, bool HorizontalFlip, bool VerticalFlip, Float InputDropout, Float Cutout, bool CutMix, Float AutoAugment, Float ColorCast, UInt ColorAngle, Float Distortion, DNNInterpolations Interpolation, Float Scaling, Float Rotation, UInt SampleIndex, Float Rate, Float Momentum, Float Beta2, Float Gamma, Float L2Penalty, Float Dropout, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors, DNNStates State, DNNTaskStates TaskState);
        public delegate void TestProgressEventDelegate(UInt BatchSize, UInt SampleIndex, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors, DNNStates State, DNNTaskStates TaskState);
        public delegate void NewEpochEventDelegate(UInt Cycle, UInt Epoch, UInt TotalEpochs, UInt Optimizer, Float Beta2, Float Gamma, Float Eps, bool HorizontalFlip, bool VerticalFlip, Float InputDropout, Float Cutout, bool CutMix, Float AutoAugment, Float ColorCast, UInt ColorAngle, Float Distortion, UInt Interpolation, Float Scaling, Float Rotation, Float Rate, UInt N, UInt D, UInt H, UInt W, UInt PadD, UInt PadH, UInt PadW, Float Momentum, Float L2Penalty, Float Dropout, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, UInt TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, UInt TestErrors, UInt ElapsedNanoSecondes);
        
		private System.Timers.Timer WorkerTimer;
		private StringBuilder sb;
		// private string oldWeightSaveFileName;

        public TrainProgressEventDelegate TrainProgress;
	    public TestProgressEventDelegate TestProgress;
		public NewEpochEventDelegate NewEpoch;

		public Byte BackgroundColor;
		public int SelectedIndex;
        public System.Collections.ObjectModel.ObservableCollection<DNNLayerInfo> Layers;
        public System.Windows.Media.Imaging.BitmapSource InputSnapshot;
        public string Label;
		public DNNCostLayer[] CostLayers;
        public Float[] MeanTrainSet;
        public Float[] StdTrainSet;
        public UInt[][] ConfusionMatrix;
        public string[][] LabelsCollection;
        public bool UseTrainingStrategy;
        public System.Collections.ObjectModel.ObservableCollection<DNNTrainingStrategy> TrainingStrategies;
        public DNNTrainingRate[] TrainingRates;
        public DNNTrainingRate TrainingRate;
		public string Definition;
		public string StorageDirectory;
		public string DatasetsDirectory;
		public string DefinitionsDirectory;
		public string Name;
		public string DurationString;
		public DNNDatasets Dataset;
		public DNNOptimizers Optimizer;
		public DNNCosts CostFunction;
		public Stopwatch Duration;
		public bool HorizontalFlip;
        public bool VerticalFlip;
        public bool MeanStdNormalization;
        public bool IsTraining;
        public UInt CostIndex;
		public UInt Hierarchies;
		public UInt ClassCount;
		public UInt GroupIndex;
		public UInt LabelIndex;
		public UInt BatchSize;
		public UInt Height;
		public UInt Width;
		public UInt PadH;
		public UInt PadW;
		public UInt LayerCount;
		public UInt Multiplier;
		public UInt CostLayerCount;
		public UInt TrainingSamples;
		public UInt AdjustedTrainSamplesCount;
		public UInt TestingSamples;
		public UInt AdjustedTestSamplesCount;
		public UInt Cycle;
		public UInt TotalCycles;
		public UInt Epoch;
		public UInt TotalEpochs;
		public Float Gamma;
		public Float ColorCast;
		public UInt ColorAngle;
		public DNNInterpolations Interpolation;
		public UInt SampleIndex;
		public UInt TrainErrors;
		public UInt TestErrors;
		public UInt BlockSize;
		public Float InputDropout;
		public Float Cutout;
		public bool CutMix;
        public Float AutoAugment;
		public Float Distortion;
		public Float Scaling;
		public Float Rotation;
		public Float AvgTrainLoss;
		public Float TrainErrorPercentage;
		public Float AvgTestLoss;
		public Float TestErrorPercentage;
		public Float Rate;
		public Float Momentum;
		public Float Beta2;
		public Float L2Penalty;
		public Float Dropout;
		public Float SampleRate;
		public DNNStates State;
		public DNNStates OldState;
		public DNNTaskStates TaskState;
		public Float fpropTime;
		public Float bpropTime;
		public Float updateTime;
		public bool PersistOptimizer;
        public bool DisableLocking;
        public bool PlainFormat;

        void OnElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            sb.Length = 0;
            if (Duration.Elapsed.Days > 0)
            {
                if (Duration.Elapsed.Days == 1)
                    sb.AppendFormat("{0:D} day {1:D2}:{2:D2}:{3:D2}", Duration.Elapsed.Days, Duration.Elapsed.Hours, Duration.Elapsed.Minutes, Duration.Elapsed.Seconds);
                else
                    sb.AppendFormat("{0:D} days {1:D2}:{2:D2}:{3:D2}", Duration.Elapsed.Days, Duration.Elapsed.Hours, Duration.Elapsed.Minutes, Duration.Elapsed.Seconds);
            }
            else
                sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", Duration.Elapsed.Hours, Duration.Elapsed.Minutes, Duration.Elapsed.Seconds);
            DurationString = sb.ToString();

            if (IsTraining)
            {
                var info = new TrainingInfo();
                DNNGetTrainingInfo(out info);

                TotalCycles = info.TotalCycles;
                TotalEpochs = info.TotalEpochs;
                Cycle = info.Cycle;
                Epoch = info.Epoch; ;
                SampleIndex = info.SampleIndex;

                Rate = info.Rate;
                if (Optimizer != (DNNOptimizers)info.Optimizer)
                    Optimizer = (DNNOptimizers)info.Optimizer;

                Momentum = info.Momentum;
                Beta2 = info.Beta2;
                L2Penalty = info.L2Penalty;
                Gamma = info.Gamma;
                Dropout = info.Dropout;
                BatchSize = info.BatchSize;
                Height = info.Height;
                Width = info.Width;
                PadH = info.PadH;
                PadW = info.PadW;

                HorizontalFlip = info.HorizontalFlip;
                VerticalFlip = info.VerticalFlip;
                InputDropout = info.InputDropout;
                Cutout = info.Cutout;
                CutMix = info.CutMix;
                AutoAugment = info.AutoAugment;
                ColorCast = info.ColorCast;
                ColorAngle = info.ColorAngle;
                Distortion = info.Distortion;
                Interpolation = (DNNInterpolations)info.Interpolation;
                Scaling = info.Scaling;
                Rotation = info.Rotation;

                AvgTrainLoss = info.AvgTrainLoss;
                TrainErrorPercentage = info.TrainErrorPercentage;
                TrainErrors = info.TrainErrors;
                AvgTestLoss = info.AvgTestLoss;
                TestErrorPercentage = info.TestErrorPercentage;
                TestErrors = info.TestErrors;

                SampleRate = info.SampleSpeed;

                State = (DNNStates)info.State;
                TaskState = (DNNTaskStates)info.TaskState;

                AdjustedTrainSamplesCount = TrainingSamples % BatchSize == 0 ? TrainingSamples : ((TrainingSamples / BatchSize) + 1) * BatchSize;
                AdjustedTestSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;

                TrainProgress(Optimizer, BatchSize, Cycle, TotalCycles, Epoch, TotalEpochs, HorizontalFlip, VerticalFlip, InputDropout, Cutout, CutMix, AutoAugment, ColorCast, ColorAngle, Distortion, Interpolation, Scaling, Rotation, SampleIndex, Rate, Momentum, Beta2, Gamma, L2Penalty, Dropout, AvgTrainLoss, TrainErrorPercentage, (Float)100 - TrainErrorPercentage, TrainErrors, AvgTestLoss, TestErrorPercentage, (Float)100 - TestErrorPercentage, TestErrors, State, TaskState);

                if (State != OldState)
                {
                    OldState = State;
                    SampleRate = (Float)0;
                }
            }
            else
            {
                var info = new TestingInfo();
                DNNGetTestingInfo(out info);

                SampleIndex = info.SampleIndex;
                BatchSize = info.BatchSize;
                Height = info.Height;
                Width = info.Width;
                PadH = info.PadH;
                PadW = info.PadW;
                AvgTestLoss = info.AvgTestLoss;
                TestErrorPercentage = info.TestErrorPercentage;
                TestErrors = info.TestErrors;
                State = (DNNStates)(info.State);
                TaskState = (DNNTaskStates)info.TaskState;
                SampleRate = info.SampleSpeed;


                AdjustedTestSamplesCount = TestingSamples % BatchSize == 0 ? TestingSamples : ((TestingSamples / BatchSize) + 1) * BatchSize;

                TestProgress(BatchSize, SampleIndex, AvgTestLoss, TestErrorPercentage, (Float)100 - TestErrorPercentage, TestErrors, State, TaskState);

                if (State != OldState)
                {
                    OldState = State;
                    SampleRate = (Float)0;
                }
            }
        }

        string[] GetTextLabels(string fileName)
	    {
	        StreamReader streamReader;
		    string? str;
		    int lines = 0;
            string[] list = new string[1];

            try
            {
                streamReader = File.OpenText(fileName);
                str = streamReader.ReadLine();
                while (str != null)
                {
                    lines++;
                    str = streamReader.ReadLine();
                }
                streamReader.Close();

                list = new string[lines];
                lines = 0;

                streamReader = File.OpenText(fileName);
                str = streamReader.ReadLine();
                while (str != null)
                { 
                    list[lines++] = new string(str);
                    str = streamReader.ReadLine();
                }
                streamReader.Close();
            }
		    catch (Exception)
		    {
            }
		
            return list;
	    }

        DNNLayerInfo GetLayerInfo(DNNLayerInfo infoManaged, UInt layerIndex)
	    {
		    if (infoManaged == null)
			    infoManaged = new DNNLayerInfo();

            var infoNative = new LayerInfo();
            DNNGetLayerInfo(layerIndex, out infoNative);

            infoManaged.Name = (string)infoNative.Name;
            infoManaged.Description = (string)infoNative.Description;

            var layerType = (DNNLayerTypes)infoNative.LayerType;
            infoManaged.LayerType = layerType;
            infoManaged.IsNormLayer =
                layerType == DNNLayerTypes.BatchNorm ||
                layerType == DNNLayerTypes.BatchNormActivation ||
                layerType == DNNLayerTypes.BatchNormActivationDropout ||
                layerType == DNNLayerTypes.BatchNormRelu ||
                layerType == DNNLayerTypes.LayerNorm;

            infoManaged.Activation = (DNNActivations)infoNative.Activation;
            infoManaged.Algorithm = (DNNAlgorithms)infoNative.Algorithm;
            infoManaged.Cost = (DNNCosts)infoNative.Cost;
            infoManaged.NeuronCount = infoNative.NeuronCount;
            infoManaged.WeightCount = infoNative.WeightCount;
            infoManaged.BiasCount = infoNative.BiasesCount;
            infoManaged.LayerIndex = layerIndex; // infoNative.LayerIndex;

            infoManaged.InputCount = infoNative.InputsCount;
            UInt[] inputs = new UInt[infoNative.InputsCount];
            DNNGetLayerInputs(layerIndex, out inputs);
            infoManaged.Inputs = new System.Collections.Generic.List<UInt>();
            foreach(UInt index in inputs)
               infoManaged.Inputs.Add(index);

            infoManaged.C = infoNative.C;
            infoManaged.D = infoNative.D;
            infoManaged.H = infoNative.H;
            infoManaged.W = infoNative.W;
            infoManaged.PadD = infoNative.PadD;
            infoManaged.PadH = infoNative.PadH;
            infoManaged.PadW = infoNative.PadW;
            infoManaged.KernelH = infoNative.KernelH;
            infoManaged.KernelW = infoNative.KernelW;
            infoManaged.StrideH = infoNative.StrideH;
            infoManaged.StrideW = infoNative.StrideW;
            infoManaged.DilationH = infoNative.DilationH;
            infoManaged.DilationW = infoNative.DilationW;
            infoManaged.Multiplier = infoNative.Multiplier;
            infoManaged.Groups = infoNative.Groups;
            infoManaged.Group = infoNative.Group;
            infoManaged.LocalSize = infoNative.LocalSize;
            infoManaged.Dropout = infoNative.Dropout;
            infoManaged.Weight = infoNative.Weight;
            infoManaged.GroupIndex = infoNative.GroupIndex;
            infoManaged.LabelIndex = infoNative.LabelIndex;
            infoManaged.InputC = infoNative.InputC;
            infoManaged.Alpha = infoNative.Alpha;
            infoManaged.Beta = infoNative.Beta;
            infoManaged.K = infoNative.K;
            infoManaged.FactorH = infoNative.fH;
            infoManaged.FactorW = infoNative.fW;
            infoManaged.HasBias = infoNative.HasBias;
            infoManaged.Scaling = infoManaged.IsNormLayer ? infoNative.Scaling : false;
            infoManaged.AcrossChannels = infoNative.AcrossChannels;
            infoManaged.LockUpdate = infoNative.Lockable ? (Nullable<bool>)infoNative.Locked : (Nullable<bool>)false;
            infoManaged.Lockable = infoNative.Lockable;

            return infoManaged;
	    }

        void ApplyParameters()
        {
            var info = new ModelInfo();
            DNNGetModelInfo(out info);

            Name = info.Name;
            Dataset = (DNNDatasets)info.Dataset;
            CostFunction = (DNNCosts)info.CostFunction;
            LayerCount = info.LayerCount;
            CostLayerCount = info.CostLayerCount;
            CostIndex = info.CostIndex;
            GroupIndex = info.GroupIndex;
            LabelIndex = info.LabelIndex;
            Hierarchies = info.Hierarchies;
            TrainingSamples = info.TrainSamplesCount;
            TestingSamples = info.TestSamplesCount;
            MeanStdNormalization = info.MeanStdNormalization;

            LabelsCollection = new string[Hierarchies][];

            switch (Dataset)
            {
                case DNNDatasets.tinyimagenet:
                    LabelsCollection[0] = GetTextLabels(string.Concat(DatasetsDirectory, Dataset.ToString() + "\\classnames.txt"));
                    /*LabelsCollection[0] = new string[200];
                    for (int i = 0; i < 200; i++)
                        LabelsCollection[0][i] = i.ToString();*/
                    if (info.MeanTrainSet.Length >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            MeanTrainSet[i] = info.MeanTrainSet[i];
                            StdTrainSet[i] = info.StdTrainSet[i];
                        }
                    }
                    break;

                case DNNDatasets.cifar10:
                    LabelsCollection[0] = GetTextLabels(string.Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
                    if (info.MeanTrainSet.Length >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            MeanTrainSet[i] = info.MeanTrainSet[i];
                            StdTrainSet[i] = info.StdTrainSet[i];
                        }
                    }
                    break;

                case DNNDatasets.cifar100:
                    LabelsCollection[0] = GetTextLabels(string.Concat(DatasetsDirectory, Dataset.ToString() + "\\coarse_label_names.txt"));
                    LabelsCollection[1] = GetTextLabels(string.Concat(DatasetsDirectory, Dataset.ToString() + "\\fine_label_names.txt"));
                    if (info.MeanTrainSet.Length >= 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            MeanTrainSet[i] = info.MeanTrainSet[i];
                            StdTrainSet[i] = info.StdTrainSet[i];
                        }
                    }
                    break;

                case DNNDatasets.fashionmnist:
                    LabelsCollection[0] = GetTextLabels(string.Concat(DatasetsDirectory, Dataset.ToString() + "\\batches.meta.txt"));
                    if (info.MeanTrainSet.Length >= 1)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            MeanTrainSet[i] = info.MeanTrainSet[i];
                            StdTrainSet[i] = info.StdTrainSet[i];
                        }
                    }
                    break;

                case DNNDatasets.mnist:
                    LabelsCollection[0] = new string[10];
                    for (int i = 0; i < 10; i++)
                        LabelsCollection[0][i] = i.ToString();
                    if (info.MeanTrainSet.Length >= 1)
                    {
                        for (int i = 0; i < 1; i++)
                        {
                            MeanTrainSet[i] = info.MeanTrainSet[i];
                            StdTrainSet[i] = info.StdTrainSet[i];
                        }
                    }
                    break;
            }

            Layers = new System.Collections.ObjectModel.ObservableCollection< DNNLayerInfo>();
            TrainingStrategies = new System.Collections.ObjectModel.ObservableCollection< DNNTrainingStrategy>();
            CostLayers = new DNNCostLayer[CostLayerCount];

            UInt counter = 0;
            for (UInt layer = 0; layer < LayerCount; layer++)
            {
                Layers.Add(GetLayerInfo(null, layer));

                if (Layers[(int)layer].LayerType == DNNLayerTypes.Cost)
                    CostLayers[counter++] = new DNNCostLayer(Layers[(int)layer].Cost, Layers[(int)layer].LayerIndex, Layers[(int)layer].GroupIndex, Layers[(int)layer].LabelIndex, Layers[(int)layer].NeuronCount, Layers[(int)layer].Name, Layers[(int)layer].Weight);
            }

            GroupIndex = CostLayers[CostIndex].GroupIndex;
            LabelIndex = CostLayers[CostIndex].LabelIndex;
            ClassCount = CostLayers[CostIndex].ClassCount;

            Optimizer = (DNNOptimizers)GetOptimizer();
        }

        public DNNModel(string definition)
	    {
		    Duration = new System.Diagnostics.Stopwatch();
            sb = new System.Text.StringBuilder();
            State = DNNStates.Idle;
		    OldState = DNNStates.Idle;
		    TaskState = DNNTaskStates.Stopped;
		    MeanTrainSet = new Float []{ (Float)0, (Float)0, (Float)0 };
            StdTrainSet = new Float[] { (Float)0, (Float)0, (Float)0 };
            StorageDirectory = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\convnet\\");
		    DatasetsDirectory = string.Concat(StorageDirectory, "datasets\\");
		    DefinitionsDirectory = string.Concat(StorageDirectory, "definitions\\");

	        GroupIndex = 0;
		    LabelIndex = 0;
		    CostIndex = 0;
		    Multiplier = 1;

		    DNNDataprovider(StorageDirectory);

            var checkMsg = new CheckMsg();
		    if (DNNRead(definition, out checkMsg) == 1)
		    {
			    DNNLoadDataset();
                Definition = definition;
			    ApplyParameters();

                WorkerTimer = new System.Timers.Timer(1000.0);
                WorkerTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnElapsed);
            }
		    else
		    {
			    throw new Exception(checkMsg.Message);
		    }
	    }
    };

}