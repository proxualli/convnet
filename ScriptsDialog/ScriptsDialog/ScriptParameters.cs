using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Float = System.Single;
using UInt = System.UInt64;


namespace ScriptsDialog
{
    [Serializable()]
    public enum Scripts
    {
        densenet = 0,
        efficientnetv2 = 1,
        mobilenetv3 = 2,
        resnet = 3,
        shufflenetv2 = 4
    }

    [Serializable()]
    public enum Datasets
    {
        cifar10 = 0,
        cifar100 = 1,
        fashionmnist = 2,
        mnist = 3,
        tinyimagenet = 4
    }

    [Serializable()]
    public enum Fillers
    {
        Constant = 0,
        HeNormal = 1,
        HeUniform = 2,
        LeCunNormal = 3,
        LeCunUniform = 4,
        Normal = 5,
        TruncatedNormal = 6,
        Uniform = 7,
        XavierNormal = 8,
        XavierUniform = 9
    }

    [Serializable()]
    public enum FillerModes
    {
        Avg = 0,
        In = 1,
        Out = 2
    }

    [Serializable()]
    public enum Activations
    {
        FRelu = 1,
        HardSwish = 10,
        HardLogistic = 11,
        Logistic = 12,
        Mish = 16,
        Relu = 19,
        Swish = 25,
        TanhExp = 27
    }

    [Serializable()]
    public class EfficientNetRecord : INotifyPropertyChanged
    {
        private UInt expandRatio;
        private UInt channels;
        private UInt iterations;
        private UInt stride;
        private bool se;

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        public EfficientNetRecord()
        {
            ExpandRatio = 4;
            Channels = 24;
            Iterations = 2;
            Stride = 1;
            SE = false;
        }

        public EfficientNetRecord(UInt expandRatio = 4, UInt channels = 24, UInt iterations = 2, UInt stride = 1, bool se = false)
        {
            this.expandRatio = expandRatio;
            this.channels = channels;
            this.iterations = iterations;
            this.stride = stride;
            this.se = se;
        }

        public override string ToString()
        {
            return "(" + ExpandRatio.ToString() + "-" + Channels.ToString() + "-" + Iterations.ToString() + "-" + Stride.ToString() + (SE ? "-se" : "") + ")";
        }

        public UInt ExpandRatio
        {
            get { return expandRatio; }
            set { expandRatio = value; OnPropertyChanged("ExpandRatio"); }
        }

        public UInt Channels
        {
            get { return channels; }
            set { channels = value; OnPropertyChanged("Channels"); }
        }

        public UInt Iterations
        {
            get { return iterations; }
            set { iterations = value; OnPropertyChanged("Iterations"); }
        }

        public UInt Stride
        {
            get { return stride; }
            set { stride = value; OnPropertyChanged("Stride"); }
        }

        public bool SE
        {
            get { return se; }
            set { se = value; OnPropertyChanged("SE"); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable()]
    public class ShuffleNetRecord : INotifyPropertyChanged
    {
        private UInt iterations;
        private UInt kernel;
        private UInt pad;
        private UInt shuffle;
        private bool se;

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        public ShuffleNetRecord()
        {
            Iterations = 6ul;
            Kernel = 3ul;
            Pad = 1ul;
            Shuffle = 2ul;
            SE = false;
        }

        public ShuffleNetRecord(UInt iterations = 6ul, UInt kernel = 3ul, UInt pad = 1ul, UInt shuffle = 2ul, bool se = false)
        {
            this.iterations = iterations;
            this.kernel = kernel;
            this.pad = pad;
            this.shuffle = shuffle;
            this.se = se;
        }

        public override string ToString()
        {
            return "(" + Iterations.ToString() + "-" + Kernel.ToString() + "-" + Pad.ToString() + "-" + Shuffle.ToString() + (SE ? "-se" : "") + ")";
        }

        public UInt Iterations
        {
            get { return iterations; }
            set { iterations = value; OnPropertyChanged("Iterations"); }
        }

        public UInt Kernel
        {
            get { return kernel; }
            set { kernel = value; OnPropertyChanged("Kernel"); }
        }

        public UInt Pad
        {
            get { return pad; }
            set { pad = value; OnPropertyChanged("Pad"); }
        }

        public UInt Shuffle
        {
            get { return shuffle; }
            set { shuffle = value; OnPropertyChanged("Shuffle"); }
        }

        public bool SE
        {
            get { return se; }
            set { se = value; OnPropertyChanged("SE"); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Serializable()]
    public class ScriptParameters : INotifyPropertyChanged
    {
        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        private Scripts script = Scripts.shufflenetv2;
        private Datasets dataset = Datasets.cifar10;
        private UInt c = 3;
        private UInt h = 32;
        private UInt w = 32;
        private UInt padH = 4;
        private UInt padW = 4;
        private bool mirrorPad = false;
        private bool meanStdNormalization = true;
        private Fillers weightsFiller = Fillers.HeNormal;
        private FillerModes weightsFillerMode = FillerModes.In;
        private Float weightsGain = 1;
        private Float weightsScale = (Float)0.05;
        private Float weightsLRM = 1;
        private Float weightsWDM = 1;
        private bool hasBias = false;
        private Fillers biasesFiller = Fillers.Constant;
        private FillerModes biasesFillerMode = FillerModes.In;
        private Float biasesGain = 1;
        private Float biasesScale = 0;
        private Float biasesLRM = 1;
        private Float biasesWDM = 1;
        private Float batchNormMomentum = (Float)0.995;
        private Float batchNormEps = (Float)1E-04;
        private bool batchNormScaling = false;
        private Float alpha = 0;
        private Float beta = 0;
        private UInt groups = 3;
        private UInt iterations = 4;
        private UInt width = 8;
        private UInt growthRate = 12;
        private bool bottleneck = false;
        private Float dropout = 0;
        private Float compression = 0;
        private bool squeezeExcitation = false;
        private bool channelZeroPad = true;
        private Activations activation = Activations.HardSwish;
        private ObservableCollection<EfficientNetRecord> efficientnet = new ObservableCollection<EfficientNetRecord>{
            new EfficientNetRecord(1, 24, 2, 1, false),
            new EfficientNetRecord(4, 48, 4, 2, false),
            new EfficientNetRecord(4, 64, 4, 2, false),
            new EfficientNetRecord(6, 160, 9, 1, true),
            new EfficientNetRecord(6, 256, 15, 2, true)};

        private ObservableCollection<ShuffleNetRecord> shufflenet = new ObservableCollection<ShuffleNetRecord>{
            new ShuffleNetRecord(5, 3, 1, 2, false),
            new ShuffleNetRecord(6, 3, 1, 2, false),
            new ShuffleNetRecord(7, 3, 1, 2, true),
            new ShuffleNetRecord(8, 3, 1, 2, true)};
        private UInt strideHFirstConv = 1;
        private UInt strideWFirstConv = 1;
        private Float depthDrop = (Float)0;
        private bool fixedDepthDrop = false;

        public ScriptParameters()
        {
        }

        public ScriptParameters(Scripts script = Scripts.shufflenetv2, Datasets dataset = Datasets.cifar10, UInt h = 32, UInt w = 32, UInt padH = 4, UInt padW = 4, bool mirrorPad = false, bool meanStdNorm = true, Fillers weightsFiller = Fillers.HeNormal, FillerModes weightsFillerMode = FillerModes.In, Float weightsGain = (Float)1.0, Float weightsScale = (Float)0.05, Float weightsLRM = 1, Float weightsWDM = 1, bool hasBias = false, Fillers biasesFiller = Fillers.Constant, FillerModes biasesFillerMode = FillerModes.In, Float biasesGain = (Float)1.0, Float biasesScale = 0, Float biasesLRM = 1, Float biasesWDM = 1, Float batchNormMomentum = (Float)0.995, Float batchNormEps = (Float)1E-04, bool batchNormScaling = false, Float alpha = (Float)0, Float beta = (Float)0, UInt groups = 3, UInt iterations = 4, UInt width = 8, UInt growthRate = 12, bool bottleneck = false, Float dropout = 0, Float compression = 0, bool squeezeExcitation = false, bool channelZeroPad = true, Activations activation = Activations.HardLogistic, UInt strideHFirstConv = 1, UInt strideWFirstConv = 1, Float depthDrop = (Float)0, bool fixedDepthDrop = false)
        {
            Script = script;
            Dataset = dataset;
            H = h;
            W = w;
            PadH = padH;
            PadW = padW;
            MirrorPad = mirrorPad;
            MeanStdNormalization = meanStdNorm;
            WeightsFiller = weightsFiller;
            WeightsFillerMode = weightsFillerMode;
            WeightsGain = weightsGain;
            WeightsScale = weightsScale;
            WeightsLRM = weightsLRM;
            WeightsWDM = weightsWDM;
            HasBias = hasBias;
            BiasesFiller = biasesFiller;
            BiasesFillerMode = biasesFillerMode;
            BiasesGain = biasesGain;
            BiasesScale = biasesScale;
            BiasesLRM = biasesLRM;
            BiasesWDM = biasesWDM;
            BatchNormMomentum = batchNormMomentum;
            BatchNormEps = batchNormEps;
            BatchNormScaling = batchNormScaling;
            Alpha = alpha;
            Beta = beta;
            Groups = groups;
            Iterations = iterations;
            Width = width;
            GrowthRate = growthRate;
            Bottleneck = bottleneck;
            Dropout = dropout;
            Compression = compression;
            SqueezeExcitation = squeezeExcitation;
            ChannelZeroPad = channelZeroPad;
            Activation = activation;
            StrideHFirstConv = strideHFirstConv;
            StrideWFirstConv = strideWFirstConv;
            DepthDrop = depthDrop;
            FixedDepthDrop = fixedDepthDrop;
        }

        public IEnumerable<Scripts> ScriptsList { get { return Enum.GetValues(typeof(Scripts)).Cast<Scripts>(); } }

        public IEnumerable<Datasets> DatasetsList { get { return Enum.GetValues(typeof(Datasets)).Cast<Datasets>(); } }

        public IEnumerable<Activations> ActivationsList { get { return Enum.GetValues(typeof(Activations)).Cast<Activations>(); } }

        public IEnumerable<Fillers> FillersList { get { return Enum.GetValues(typeof(Fillers)).Cast<Fillers>(); } }

        public IEnumerable<FillerModes> FillerModesList { get { return Enum.GetValues(typeof(FillerModes)).Cast<FillerModes>(); } }

        public string ModelName
        {
            get
            {
                switch (Script)
                {
                    case Scripts.densenet:
                        return Script.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + GrowthRate.ToString() + (Dropout > 0 ? "-dropout" : "") + (DepthDrop > 0 ? (FixedDepthDrop ? "-fixeddepthdrop" : "-depthdrop") : "") + (Compression > 0 ? "-compression" : "") + (Bottleneck ? "-bottleneck" : "") + "-" + Activation.ToString().ToLower();
                    case Scripts.efficientnetv2:
                        {
                            string name = "";
                            foreach (var rec in EfficientNet)
                                name += rec.ToString();
                            return Script.ToString() + (DepthDrop > 0 ? (FixedDepthDrop ? "-fixeddepthdrop-" : "-depthdrop-") : "") + name;
                        }
                    case Scripts.mobilenetv3:
                        return Script.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + Width.ToString() + "-" + Activation.ToString().ToLower() + (SqueezeExcitation ? " -se" : "") + (DepthDrop > 0 ? (FixedDepthDrop ? "-fixeddepthdrop" : "-depthdrop") : "");
                    case Scripts.resnet:
                        return Script.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString() + "-" + Width.ToString() + (Dropout > 0 ? "-dropout" : "") + (DepthDrop > 0 ? (FixedDepthDrop ? "-fixeddepthdrop" : "-depthdrop") : "") + (Bottleneck ? "-bottleneck" : "") + (ChannelZeroPad ? "-channelzeropad" : "") + "-" + Activation.ToString().ToLower();
                    case Scripts.shufflenetv2:
                        {
                            string name = "";
                            foreach (var rec in ShuffleNet)
                                name += rec.ToString();
                            return Script.ToString() + "-" + Width.ToString() + name;
                        }
                    default:
                        return Script.ToString() + "-" + Groups.ToString() + "-" + Iterations.ToString();
                }
            }
        }

        public Scripts Script
        {
            get { return script; }
            set
            {
                if (value != script)
                {
                    script = value;
                    OnPropertyChanged("Script");
                    OnPropertyChanged("Classes");
                    OnPropertyChanged("Depth");
                    OnPropertyChanged("WidthVisible");
                    OnPropertyChanged("GrowthRateVisible");
                    OnPropertyChanged("DropoutVisible");
                    OnPropertyChanged("CompressionVisible");
                    OnPropertyChanged("BottleneckVisible");
                    OnPropertyChanged("ChannelZeroPadVisible");
                    OnPropertyChanged("SqueezeExcitationVisible");
                    OnPropertyChanged("BottleneckVisible");
                    OnPropertyChanged("EfficientNetVisible");
                    OnPropertyChanged("ShuffleNetVisible");
                    OnPropertyChanged("DepthDropVisible");
                }
            }
        }

        public UInt Classes
        {
            get
            {
                switch (Dataset)
                {
                    case Datasets.cifar100:
                        return 100;
                    case Datasets.tinyimagenet:
                        return 200;
                    default:
                        return 10;
                }
            }
        }

        public Datasets Dataset
        {
            get { return dataset; }
            set
            {
                if (value != dataset)
                {
                    dataset = value;
                    OnPropertyChanged("Dataset");
                    OnPropertyChanged("C");
                }
            }
        }

        public UInt C
        {
            get
            {
                switch (dataset)
                {
                    case Datasets.cifar10:
                    case Datasets.cifar100:
                    case Datasets.tinyimagenet:
                        return 3;
                    case Datasets.fashionmnist:
                    case Datasets.mnist:
                        return 1;
                    default:
                        return 1;
                }
            }
            set
            {
                if (value != c)
                {
                    c = value;
                    OnPropertyChanged("C");
                }
            }
        }

        public UInt D
        {
            get { return 1; }
        }

        public UInt H
        {
            get { return h; }
            set
            {
                if (value >= 14 && value <= 4096 && value != h)
                {
                    h = value;
                    OnPropertyChanged("H");
                }
            }
        }

        public UInt W
        {
            get { return w; }
            set
            {
                if (value >= 14 && value <= 4096 && value != w)
                {
                    w = value;
                    OnPropertyChanged("W");
                }
            }
        }

        public UInt PadD
        {
            get { return 0; }
        }

        public UInt PadH
        {
            get { return padH; }
            set
            {
                if (value >= 0 && value <= H && value != padH)
                {
                    padH = value;
                    OnPropertyChanged("PadH");
                    OnPropertyChanged("RandomCrop");
                }
            }
        }

        public UInt PadW
        {
            get { return padW; }
            set
            {
                if (value >= 0 && value <= W && value != padW)
                {
                    padW = value;
                    OnPropertyChanged("PadW");
                    OnPropertyChanged("RandomCrop");
                }
            }
        }

        public bool MirrorPad
        {
            get { return mirrorPad; }
            set
            {
                if (value != mirrorPad)
                {
                    mirrorPad = value;
                    OnPropertyChanged("MirrorPad");
                }
            }
        }

        public bool RandomCrop
        {
            get { return padH > 0 || padW > 0; }
        }

        public bool MeanStdNormalization
        {
            get { return meanStdNormalization; }
            set
            {
                if (value != meanStdNormalization)
                {
                    meanStdNormalization = value;
                    OnPropertyChanged("MeanStdNormalization");
                }
            }
        }

        public bool FixedDepthDrop
        {
            get { return fixedDepthDrop; }
            set
            {
                if (value != fixedDepthDrop)
                {
                    fixedDepthDrop = value;
                    OnPropertyChanged("FixedDepthDrop");
                }
            }
        }

        public Float DepthDrop
        {
            get { return depthDrop; }
            set
            {
                if (value >= 0 && value <= 1 && value != depthDrop)
                {
                    depthDrop = value;
                    OnPropertyChanged("DepthDrop");
                }
            }
        }

        public Fillers WeightsFiller
        {
            get { return weightsFiller; }
            set
            {
                if (value != weightsFiller)
                {
                    weightsFiller = value;
                    OnPropertyChanged("WeightsFiller");
                    OnPropertyChanged("WeightsFillerModeVisible");
                    OnPropertyChanged("WeightsGainVisible");
                    OnPropertyChanged("WeightsScaleVisible");
                }
            }
        }

        public FillerModes WeightsFillerMode
        {
            get { return weightsFillerMode; }
            set
            {
                if (value != weightsFillerMode)
                {
                    weightsFillerMode = value;
                    OnPropertyChanged("WeightsFillerMode");
                }
            }
        }

        public Float WeightsGain
        {
            get { return weightsGain; }
            set
            {
                if (value != weightsGain)
                {
                    weightsGain = value;
                    OnPropertyChanged("WeightsGain");
                }
            }
        }

        public bool WeightsFillerModeVisible
        {
            get
            {
                return WeightsFiller == Fillers.HeNormal || WeightsFiller == Fillers.HeUniform || WeightsFiller == Fillers.LeCunNormal || WeightsFiller == Fillers.LeCunUniform;
            }
        }

        public bool WeightsGainVisible
        {
            get
            {
                return WeightsFiller == Fillers.HeNormal || WeightsFiller == Fillers.HeUniform || WeightsFiller == Fillers.LeCunNormal || WeightsFiller == Fillers.LeCunUniform || WeightsFiller == Fillers.XavierNormal || WeightsFiller == Fillers.XavierUniform;
            }
        }

        public bool WeightsScaleVisible
        {
            get
            {
                return WeightsFiller == Fillers.Constant || WeightsFiller == Fillers.Normal || WeightsFiller == Fillers.TruncatedNormal || WeightsFiller == Fillers.Uniform;
            }
        }

        public Float WeightsScale
        {
            get { return weightsScale; }
            set
            {
                if (value != weightsScale)
                {
                    weightsScale = value;
                    OnPropertyChanged("WeightsScale");
                }
            }
        }

        public Float WeightsLRM
        {
            get { return weightsLRM; }
            set
            {
                if (value != weightsLRM)
                {
                    weightsLRM = value;
                    OnPropertyChanged("WeightsLRM");
                }
            }
        }

        public Float WeightsWDM
        {
            get { return weightsWDM; }
            set
            {
                if (value != weightsWDM)
                {
                    weightsWDM = value;
                    OnPropertyChanged("WeightsWDM");
                }
            }
        }

        public bool HasBias
        {
            get { return hasBias; }
            set
            {
                if (value != hasBias)
                {
                    hasBias = value;
                    OnPropertyChanged("HasBias");
                }
            }
        }

        public Fillers BiasesFiller
        {
            get { return biasesFiller; }
            set
            {
                if (value != biasesFiller)
                {
                    biasesFiller = value;
                    OnPropertyChanged("BiasesFiller");
                    OnPropertyChanged("BiasesFillerModeVisible");
                    OnPropertyChanged("BiasesGainVisible");
                    OnPropertyChanged("BiasesScaleVisible");
                }
            }
        }

        public FillerModes BiasesFillerMode
        {
            get { return biasesFillerMode; }
            set
            {
                if (value != biasesFillerMode)
                {
                    biasesFillerMode = value;
                    OnPropertyChanged("BiasesFillerMode");
                }
            }
        }

        public Float BiasesGain
        {
            get { return biasesGain; }
            set
            {
                if (value != biasesGain)
                {
                    biasesGain = value;
                    OnPropertyChanged("BiasesGain");
                }
            }
        }

        public Float BiasesScale
        {
            get { return biasesScale; }
            set
            {
                if (value != biasesScale)
                {
                    biasesScale = value;
                    OnPropertyChanged("BiasesScale");
                }
            }
        }

        public bool BiasesFillerModeVisible
        {
            get
            {
                return BiasesFiller == Fillers.HeNormal || BiasesFiller == Fillers.HeUniform || BiasesFiller == Fillers.LeCunNormal || BiasesFiller == Fillers.LeCunUniform;
            }
        }

        public bool BiasesGainVisible
        {
            get
            {
                return BiasesFiller == Fillers.HeNormal || BiasesFiller == Fillers.HeUniform || BiasesFiller == Fillers.LeCunNormal || BiasesFiller == Fillers.LeCunUniform || BiasesFiller == Fillers.XavierNormal || BiasesFiller == Fillers.XavierUniform;
            }
        }

        public bool BiasesScaleVisible
        {
            get
            {
                return BiasesFiller == Fillers.Constant || BiasesFiller == Fillers.Normal || BiasesFiller == Fillers.TruncatedNormal || BiasesFiller == Fillers.Uniform;
            }
        }

        public Float BiasesLRM
        {
            get { return biasesLRM; }
            set
            {
                if (value != biasesLRM)
                {
                    biasesLRM = value;
                    OnPropertyChanged("BiasesLRM");
                }
            }
        }

        public Float BiasesWDM
        {
            get { return biasesWDM; }
            set
            {
                if (value != biasesWDM)
                {
                    biasesWDM = value;
                    OnPropertyChanged("BiasesWDM");
                }
            }
        }

        public Float BatchNormMomentum
        {
            get { return batchNormMomentum; }
            set
            {
                if (value != batchNormMomentum)
                {
                    batchNormMomentum = value;
                    OnPropertyChanged("BatchNormMomentum");
                }
            }
        }

        public Float BatchNormEps
        {
            get { return batchNormEps; }
            set
            {
                if (value != batchNormEps)
                {
                    batchNormEps = value;
                    OnPropertyChanged("BatchNormEps");
                }
            }
        }

        public bool BatchNormScaling
        {
            get { return batchNormScaling; }
            set
            {
                if (value != batchNormScaling)
                {
                    batchNormScaling = value;
                    OnPropertyChanged("BatchNormScaling");
                }
            }
        }

        public Float Alpha
        {
            get { return alpha; }
            set
            {
                if (value != alpha)
                {
                    if (value >= 0 && value <= 1)
                    {
                        alpha = value;
                        OnPropertyChanged("Alpha");
                    }
                }
            }
        }

        public Float Beta
        {
            get { return beta; }
            set
            {
                if (value != beta)
                {
                    if (value >= 0 && value <= 1)
                    {
                        beta = value;
                        OnPropertyChanged("Beta");
                    }
                }
            }
        }

        public UInt Groups
        {
            get { return groups; }
            set
            {
                if (value >= 1 && value <= 6 && value != groups)
                {
                    groups = value;
                    OnPropertyChanged("Groups");
                    OnPropertyChanged("Depth");
                }
            }
        }
        public UInt Iterations
        {
            get { return iterations; }
            set
            {
                if (value >= 2 && value <= 100 && value != iterations)
                {
                    iterations = value;
                    OnPropertyChanged("Iterations");
                    OnPropertyChanged("Depth");
                }
            }
        }

        public UInt Depth
        {
            get
            {
                switch (Script)
                {
                    case Scripts.densenet:
                        return (Groups * Iterations * (Bottleneck ? 2u : 1u)) + ((Groups - 1) * 2);
                    case Scripts.mobilenetv3:
                        return (Groups * Iterations * 3) + ((Groups - 1) * 2);
                    case Scripts.resnet:
                        return (Groups * Iterations * (Bottleneck ? 3u : 2u)) + ((Groups - 1) * 2);
                    default:
                        return 0;
                }
            }
        }

        public UInt Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    OnPropertyChanged("Width");
                }
            }
        }

        public UInt GrowthRate
        {
            get { return growthRate; }
            set
            {
                if (value != growthRate)
                {
                    growthRate = value;
                    OnPropertyChanged("GrowthRate");
                }
            }
        }

        public bool Bottleneck
        {
            get { return bottleneck; }
            set
            {
                if (value != bottleneck)
                {
                    bottleneck = value;
                    OnPropertyChanged("Bottleneck");
                    OnPropertyChanged("Depth");
                }
            }
        }

        public Float Dropout
        {
            get { return dropout; }
            set
            {
                if (value != dropout)
                {
                    if (value >= 0 && value < 1)
                    {
                        dropout = value;
                        OnPropertyChanged("Dropout");
                    }
                }
            }
        }

        public Float Compression
        {
            get { return compression; }
            set
            {
                if (value != compression)
                {
                    if (value >= 0 && value <= 1)
                    {
                        compression = value;
                        OnPropertyChanged("Compression");
                    }
                }
            }
        }

        public ObservableCollection<EfficientNetRecord> EfficientNet
        {
            get { return efficientnet; }
            set
            {
                if (value != efficientnet)
                {
                    efficientnet = value;
                    OnPropertyChanged("EfficientNet");
                }
            }
        }

        public ObservableCollection<ShuffleNetRecord> ShuffleNet
        {
            get { return shufflenet; }
            set
            {
                if (value != shufflenet)
                {
                    shufflenet = value;
                    OnPropertyChanged("ShuffleNet");
                }
            }
        }

        public bool DropoutUsed
        {
            get { return (dropout > 0 && dropout < 1); }
        }

        public bool SqueezeExcitation
        {
            get { return squeezeExcitation; }
            set
            {
                if (value != squeezeExcitation)
                {
                    squeezeExcitation = value;
                    OnPropertyChanged("SqueezeExcitation");
                }
            }
        }

        public bool ChannelZeroPad
        {
            get { return channelZeroPad; }
            set
            {
                if (value != channelZeroPad)
                {
                    channelZeroPad = value;
                    OnPropertyChanged("ChannelZeroPad");
                }
            }
        }

        public Activations Activation
        {
            get { return activation; }
            set
            {
                if (value != activation)
                {
                    activation = value;
                    OnPropertyChanged("Activation");
                }
            }
        }

        public UInt StrideHFirstConv
        {
            get { return strideHFirstConv; }
            set
            {
                if (value != strideHFirstConv && value > 0)
                {
                    strideHFirstConv = value;
                    OnPropertyChanged("StrideHFirstConv");
                }
            }
        }

        public UInt StrideWFirstConv
        {
            get { return strideWFirstConv; }
            set
            {
                if (value != strideWFirstConv && value > 0)
                {
                    strideWFirstConv = value;
                    OnPropertyChanged("StrideWFirstConv");
                }
            }
        }

        public bool GroupsVisible { get { return Script != Scripts.efficientnetv2 && Script != Scripts.shufflenetv2; } }
        public bool IterationsVisible { get { return Script != Scripts.efficientnetv2 && Script != Scripts.shufflenetv2; } }
        public bool WidthVisible { get { return Script == Scripts.mobilenetv3 || Script == Scripts.resnet || Script == Scripts.shufflenetv2; } }
        public bool GrowthRateVisible { get { return Script == Scripts.densenet; } }
        public bool DropoutVisible { get { return Script == Scripts.densenet || Script == Scripts.resnet || Script == Scripts.efficientnetv2; } }
        public bool DepthDropVisible { get { return Script == Scripts.efficientnetv2 || Script == Scripts.mobilenetv3 || Script == Scripts.resnet || Script == Scripts.densenet; } }
        public bool CompressionVisible { get { return Script == Scripts.densenet; } }
        public bool BottleneckVisible { get { return Script == Scripts.densenet || Script == Scripts.resnet; } }
        public bool SqueezeExcitationVisible { get { return Script == Scripts.mobilenetv3; } }
        public bool ChannelZeroPadVisible { get { return Script == Scripts.resnet; } }
        public bool EfficientNetVisible { get { return Script == Scripts.efficientnetv2; } }
        public bool ShuffleNetVisible { get { return Script == Scripts.shufflenetv2; } }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}