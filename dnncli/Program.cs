using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;
// using System.Windows.Media;  use ImageSharp nuget
//using msclr.interop;

namespace dnncli
{
    using Float = System.Single;
    using Byte = System.Byte;
	using size_t = System.UInt64;


	[Serializable()]
	public enum DNNAlgorithms
	{
		Linear = 0,
		Nearest = 1
	}

	[Serializable()]
	public enum DNNInterpolation
	{
		Cubic = 0,
		Linear = 1,
		Nearest = 2
	}

	[Serializable()]
	public enum DNNOptimizers
	{
		AdaDelta = 0,
		AdaGrad = 1,
		Adam = 2,
		Adamax = 3,
		NAG = 4,
		RMSProp = 5,
		SGD = 6,
		SGDMomentum = 7,
		RAdam = 8
	}

	[Serializable()]
	public enum DNNDatasets
	{
		cifar10 = 0,
		cifar100 = 1,
		fashionmnist = 2,
		mnist = 3,
		tinyimagenet = 4
	}

	[Serializable()]
	public enum DNNScripts
	{
		densenet = 0,
		mobilenetv3 = 1,
		resnet = 2,
		shufflenetv2 = 3
	}

	[Serializable()]
	public enum DNNCosts
	{
		BinaryCrossEntropy = 0,
		CategoricalCrossEntropy = 1,
		MeanAbsoluteEpsError = 2,
		MeanAbsoluteError = 3,
		MeanSquaredError = 4,
		SmoothHinge = 5
	}

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
		Uniform = 7,
		XavierNormal = 8,
		XavierUniform = 9
	}

	[Serializable()]
	public enum DNNLayerTypes
	{
		Activation = 0,
		Add = 1,
		Average = 2,
		AvgPooling = 3,
		BatchNorm = 4,
		BatchNormHardLogistic = 5,
		BatchNormHardSwish = 6,
		BatchNormHardSwishDropout = 7,
		BatchNormMish = 8,
		BatchNormMishDropout = 9,
		BatchNormRelu = 10,
		BatchNormReluDropout = 11,
		BatchNormSwish = 12,
		ChannelMultiply = 13,
		ChannelShuffle = 14,
		ChannelSplit = 15,
		ChannelZeroPad = 16,
		Concat = 17,
		Convolution = 18,
		ConvolutionTranspose = 19,
		Cost = 20,
		Dense = 21,
		DepthwiseConvolution = 22,
		Divide = 23,
		Dropout = 24,
		GlobalAvgPooling = 25,
		GlobalMaxPooling = 26,
		Input = 27,
		LocalResponseNormalization = 28,
		Max = 29,
		MaxPooling = 30,
		Min = 31,
		Multiply = 32,
		PartialDepthwiseConvolution = 33,
		Resampling = 34,
		Substract = 35
	}

	[Serializable()]
	public enum DNNActivations
	{
		Abs = 0,
		BoundedRelu = 1,
		Clip = 2,
		ClipV2 = 3,
		Elu = 4,
		Exp = 5,
		FTS = 6,
		Gelu = 7,
		GeluErf = 8,
		HardLogistic = 9,
		HardSwish = 10,
		Linear = 11,
		Log = 12,
		Logistic = 13,
		LogLogistic = 14,
		LogSoftmax = 15,
		Mish = 16,
		Pow = 17,
		PRelu = 18,
		Relu = 19,
		Round = 20,
		Softmax = 21,
		SoftRelu = 22,
		Sqrt = 23,
		Square = 24,
		Swish = 25,
		Tanh = 26
	}

	[Serializable()]
	public enum DNNStates
	{
		Idle = 0,
		NewEpoch = 1,
		Testing = 2,
		Training = 3,
		SaveWeights = 4,
		Completed = 5
	}

	[Serializable()]
	public enum DNNTaskStates
	{
		Paused = 0,
		Running = 1,
		Stopped = 2
	}

	[Serializable()]
	public class DNNCostLayer
	{
		public DNNCosts CostFunction;
		public size_t LayerIndex;
		public size_t GroupIndex;
		public size_t LabelIndex;
		public size_t ClassCount;
		public string Name;
		public Float Weight;

		public size_t TrainErrors;
		public Float TrainLoss;
		public Float AvgTrainLoss;
		public Float TrainErrorPercentage;
		public Float TrainAccuracy;

		public size_t TestErrors;
		public Float TestLoss;
		public Float AvgTestLoss;
		public Float TestErrorPercentage;
		public Float TestAccuracy;

		DNNCostLayer(DNNCosts costFunction, size_t layerIndex, size_t groupIndex, size_t labelIndex, size_t classCount, string name, Float weight)
		{
			CostFunction = costFunction;
			LayerIndex = layerIndex;
			GroupIndex = groupIndex;
			LabelIndex = labelIndex;
			ClassCount = classCount;
			Name = name;
			Weight = weight;

			TrainErrors = 0;
			TrainLoss = 0;
			AvgTrainLoss = 0;
			TrainErrorPercentage = 0;
			TrainAccuracy = 0;

			TestErrors = 0;
			TestLoss = 0;
			AvgTestLoss = 0;
			TestErrorPercentage = 0;
			TestAccuracy = 0;
		}
	}


	[Serializable()]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public class DNNTrainingRate : INotifyPropertyChanged
    {
		[MarshalAs(UnmanagedType.U8)]
		private size_t batchSize;
		[MarshalAs(UnmanagedType.U8)]
		private size_t cycles;
		[MarshalAs(UnmanagedType.U8)]
		private size_t epochs;
		[MarshalAs(UnmanagedType.U8)]
		private size_t epochMultiplier;
		[MarshalAs(UnmanagedType.U8)]
		private size_t decayAfterEpochs;
		[MarshalAs(UnmanagedType.U8)]
		private size_t interpolation;
		[MarshalAs(UnmanagedType.U8)]
		private size_t colorAngle;
		[MarshalAs(UnmanagedType.R4)]
		private Float colorCast;
		[MarshalAs(UnmanagedType.R4)]
		private Float distortion;
		[MarshalAs(UnmanagedType.R4)]
		private Float dropout;
		[MarshalAs(UnmanagedType.R4)]
		private Float cutout;
		[MarshalAs(UnmanagedType.R4)]
		private Float autoAugment;
		[MarshalAs(UnmanagedType.R4)]
		private Float maximumRate;
		[MarshalAs(UnmanagedType.R4)]
		private Float minimumRate;
		[MarshalAs(UnmanagedType.R4)]
		private Float l2Penalty;
		[MarshalAs(UnmanagedType.R4)]
		private Float momentum;
		[MarshalAs(UnmanagedType.R4)]
		private Float decayFactor;
		[MarshalAs(UnmanagedType.R4)]
		private Float scaling;
		[MarshalAs(UnmanagedType.R4)]
		private Float rotation;
		[MarshalAs(UnmanagedType.I1)]
		private bool horizontalFlip;
		[MarshalAs(UnmanagedType.I1)]
		private bool verticalFlip;

		[field: NonSerialized()]
		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged(string propertyName) { PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

		public size_t BatchSize
		{
			get { return batchSize; }
			set
			{
				if (value == batchSize && value == 0)
					return;

				batchSize = value;
				OnPropertyChanged("BatchSize");
			}
		}

		public size_t Cycles
		{
			get { return cycles; }
			set
			{
				if (value == cycles && value == 0)
					return;

				cycles = value;
				OnPropertyChanged("Cycles");
			}
		}

		public size_t Epochs
		{
			get { return epochs; }
			set
			{
				if (value == epochs && value == 0)
					return;

				epochs = value;
				OnPropertyChanged("Epochs");
			}
		}

		public size_t EpochMultiplier
		{
			get { return epochMultiplier; }
			set
			{
				if (value == epochMultiplier && value == 0)
					return;

				epochMultiplier = value;
				OnPropertyChanged("EpochMultiplier");
			}
		}

		public size_t DecayAfterEpochs
		{
			get { return decayAfterEpochs; }
			set
			{
				if (value == decayAfterEpochs && value == 0)
					return;

				decayAfterEpochs = value;
				OnPropertyChanged("DecayAfterEpochs");
			}
		}

		public size_t Interpolation
		{
			get { return interpolation; }
			set
			{
				if (value == interpolation)
					return;

				interpolation = value;
				OnPropertyChanged("Interpolation");
			}
		}

		public size_t ColorAngle
		{
			get { return colorAngle; }
			set
			{
				if (value == colorAngle || value > 360)
					return;

				colorAngle = value;
				OnPropertyChanged("ColorAngle");
			}
		}

		public Float ColorCast
		{
			get { return colorCast; }
			set
			{
				if (value == colorCast || value < 0 || value > 1)
					return;

				colorCast = value;
				OnPropertyChanged("ColorCast");
			}
		}

		public Float Distortion
		{
			get { return distortion; }
			set
			{
				if (value == distortion || value < 0 || value > 1)
					return;

				distortion = value;
				OnPropertyChanged("Distortion");
			}
		}

		public Float Dropout
		{
			get { return dropout; }
			set
			{
				if (value == dropout || value < 0 || value > 1)
					return;

				dropout = value;
				OnPropertyChanged("Dropout");
			}
		}

		public Float Cutout
		{
			get { return cutout; }
			set
			{
				if (value == cutout || value < 0 || value > 1)
					return;

				cutout = value;
				OnPropertyChanged("Cutout");
			}
		}

		public Float AutoAugment
		{
			get { return autoAugment; }
			set
			{
				if (value == autoAugment || value < 0 || value > 1)
					return;

				autoAugment = value;
				OnPropertyChanged("AutoAugment");
			}
		}

		public Float MaximumRate
		{
			get { return maximumRate; }
			set
			{
				if (value == maximumRate || value < 0 || value > 1)
					return;

				maximumRate = value;
				OnPropertyChanged("MaximumRate");
			}
		}

		public Float MinimumRate
		{
			get { return minimumRate; }
			set
			{
				if (value == minimumRate || value < 0 || value > 1)
					return;

				minimumRate = value;
				OnPropertyChanged("MinimumRate");
			}
		}

		public Float L2Penalty
		{
			get { return l2Penalty; }
			set
			{
				if (value == l2Penalty || value < 0 || value > 1)
					return;

				l2Penalty = value;
				OnPropertyChanged("L2Penalty");
			}
		}

		public Float Momentum
		{
			get { return momentum; }
			set
			{
				if (value == momentum || value < 0 || value > 1)
					return;

				momentum = value;
				OnPropertyChanged("Momentum");
			}
		}

		public Float DecayFactor
		{
			get { return decayFactor; }
			set
			{
				if (value == decayFactor || value < 0 || value > 1)
					return;

				decayFactor = value;
				OnPropertyChanged("DecayFactor");
			}
		}

		public Float Scaling
		{
			get { return scaling; }
			set
			{
				if (value == scaling || value <= 0 || value > 200)
					return;

				scaling = value;
				OnPropertyChanged("Scaling");
			}
		}

		public Float Rotation
		{
			get { return rotation; }
			set
			{
				if (value == rotation || value < 0 || value > 360)
					return;

				rotation = value;
				OnPropertyChanged("Rotation");
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
				OnPropertyChanged("HorizontalFlip");
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
				OnPropertyChanged("VerticalFlip");
			}
		}

		DNNTrainingRate(Float maximumRate, size_t batchSize, size_t cycles, size_t epochs, size_t epochMultiplier, Float minRate, Float l2Penalty, Float momentum, Float decayFactor, size_t decayAfterEpochs, bool horizontalFlip, bool verticalFlip, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation)
		{
            MaximumRate = maximumRate;
			BatchSize = batchSize;
			Cycles = cycles;
			Epochs = epochs;
			EpochMultiplier = epochMultiplier;
			MinimumRate = minRate;
			L2Penalty = l2Penalty;
			Momentum = momentum;
			DecayFactor = decayFactor;
			DecayAfterEpochs = decayAfterEpochs;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			Dropout = dropout;
			Cutout = cutout;
			AutoAugment = autoAugment;
			ColorCast = colorCast;
			ColorAngle = colorAngle;
			Distortion = distortion;
			Interpolation = interpolation;
			Scaling = scaling;
			Rotation = rotation;
		}
	}

	[Serializable()]
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	public class DNNTrainingResult
	{
		[MarshalAs(UnmanagedType.U8)]
		public size_t Cycle;
		[MarshalAs(UnmanagedType.U8)]
		public size_t Epoch;
		[MarshalAs(UnmanagedType.U8)]
		public size_t GroupIndex;
		[MarshalAs(UnmanagedType.U8)]
		public size_t CostIndex;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string CostName;
		[MarshalAs(UnmanagedType.R4)]
		public Float Rate;
		[MarshalAs(UnmanagedType.U8)]
		public size_t BatchSize;
		[MarshalAs(UnmanagedType.R4)]
		public Float Momentum;
		[MarshalAs(UnmanagedType.R4)]
		public Float L2Penalty;
		[MarshalAs(UnmanagedType.R4)]
		public Float Dropout;
		[MarshalAs(UnmanagedType.R4)]
		public Float Cutout;
		[MarshalAs(UnmanagedType.R4)]
		public Float AutoAugment;
		[MarshalAs(UnmanagedType.R4)]
		public Float ColorCast;
		[MarshalAs(UnmanagedType.U8)]
		public size_t ColorAngle;
		[MarshalAs(UnmanagedType.R4)]
		public Float Distortion;
		[MarshalAs(UnmanagedType.U8)]
		public size_t Interpolation;
		[MarshalAs(UnmanagedType.R4)]
		public Float Scaling;
		[MarshalAs(UnmanagedType.R4)]
		public Float Rotation;
		[MarshalAs(UnmanagedType.I1)]
		public bool HorizontalFlip;
		[MarshalAs(UnmanagedType.I1)]
		public bool VerticalFlip;
		[MarshalAs(UnmanagedType.R4)]
		public Float AvgTrainLoss;
		[MarshalAs(UnmanagedType.U8)]
		public size_t TrainErrors;
		[MarshalAs(UnmanagedType.R4)]
		public Float TrainErrorPercentage;
		[MarshalAs(UnmanagedType.R4)]
		public Float TrainAccuracy;
		[MarshalAs(UnmanagedType.R4)]
		public Float AvgTestLoss;
		[MarshalAs(UnmanagedType.U8)]
		public size_t TestErrors;
		[MarshalAs(UnmanagedType.R4)]
		public Float TestErrorPercentage;
		[MarshalAs(UnmanagedType.R4)]
		public Float TestAccuracy;
		[MarshalAs(UnmanagedType.I8)]
		public long ElapsedTicks;
		public TimeSpan ElapsedTime;

		DNNTrainingResult(size_t cycle, size_t epoch, size_t groupIndex, size_t costIndex, string costName, Float rate, size_t batchSize, Float momentum, Float l2Penalty, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation, bool horizontalFlip, bool verticalFlip, Float avgTrainLoss, size_t trainErrors, Float trainErrorPercentage, Float trainAccuracy, Float avgTestLoss, size_t testErrors, Float testErrorPercentage, Float testAccuracy, long elapsedTicks)
		{
			Cycle = cycle;
			Epoch = epoch;
			GroupIndex = groupIndex;
			CostIndex = costIndex;
			CostName = costName;
			Rate = rate;
			BatchSize = batchSize;
			Momentum = momentum;
			L2Penalty = l2Penalty;
			Dropout = dropout;
			Cutout = cutout;
			AutoAugment = autoAugment;
			ColorCast = colorCast;
			ColorAngle = colorAngle;
			Distortion = distortion;
			Interpolation = interpolation;
			Scaling = scaling;
			Rotation = rotation;
			HorizontalFlip = horizontalFlip;
			VerticalFlip = verticalFlip;
			AvgTrainLoss = avgTrainLoss;
			TrainErrors = trainErrors;
			TrainErrorPercentage = trainErrorPercentage;
			TrainAccuracy = trainAccuracy;
			AvgTestLoss = avgTestLoss;
			TestErrors = testErrors;
			TestErrorPercentage = testErrorPercentage;
			TestAccuracy = testAccuracy;
			ElapsedTicks = elapsedTicks;
			ElapsedTime = System.TimeSpan.FromTicks(elapsedTicks);
		}
	}

	[Serializable()]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public ref struct DNNCheckMsg
	{
		[MarshalAs(UnmanagedType.U8)]
		public size_t Row;
		[MarshalAs(UnmanagedType.U8)]
		public size_t Column;
		[MarshalAs(UnmanagedType.I1)]
		public bool Error;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Message;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string Definition;

		DNNCheckMsg(size_t row, size_t column, string message, bool error, string definition)
		{
			Row = row;
			Column = column;
			Message = message;
			Error = error;
			Definition = definition;
		}
	}

	public static class NativeMethods
	{
		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNStochasticEnabled();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNSetLocked(bool locked);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNSetLayerLocked(size_t layerIndex, bool locked);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNPersistOptimizer(bool persist);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNDisableLocking(bool disable);

		//[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		//public static extern void DNNGetConfusionMatrix(size_t costLayerIndex, std::vector<std::vector<size_t>>* confusionMatrix);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetLayerInputs([In] size_t layerIndex, [Out] size_t[] inputs);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		static extern void DNNGetLayerInfo(size_t layerIndex, ref size_t inputsCount, ref DNNLayerTypes layerType, ref DNNActivations activationFunction, ref DNNCosts cost, ref string name, ref string description, ref size_t neuronCount, ref size_t weightCount, ref size_t biasesCount, ref size_t multiplier, ref size_t groups, ref size_t group, ref size_t localSize, ref size_t c, ref size_t d, ref size_t h, ref size_t w, ref size_t kernelH, ref size_t kernelW, ref size_t strideH, ref size_t strideW, ref size_t dilationH, ref size_t dilationW, ref size_t padD, ref size_t padH, ref size_t padW, ref Float dropout, ref Float labelTrue, ref Float labelFalse, ref Float weight, ref size_t groupIndex, ref size_t labelIndex, ref size_t inputC, ref Float alpha, ref Float beta, ref Float k, ref DNNAlgorithms algorithm, ref Float fH, ref Float fW, ref bool hasBias, ref bool scaling, ref bool acrossChannels, ref bool locked, ref bool lockable);

		//[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		//public static extern void DNNSetNewEpochDelegate(void(* newEpoch)(size_t, size_t, size_t, bool, bool, Float, Float, Float, Float, size_t, Float, size_t, Float, Float, Float, size_t, Float, Float, Float, Float, Float, size_t, Float, Float, Float, size_t));

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNModelDispose();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNBatchNormalizationUsed();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNSetOptimizersHyperParameters(Float adaDeltaEps, Float adaGradEps, Float adamEps, Float adamBeta2, Float adamaxEps, Float adamaxBeta2, Float rmsPropEps, Float radamEps, Float radamBeta1, Float radamBeta2);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNResetWeights();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNResetLayerWeights(size_t layerIndex);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNAddLearningRate(bool clear, size_t gotoEpoch, Float maximumRate, size_t bachSize, size_t cycles, size_t epochs, size_t epochMultiplier, Float minimumRate, Float L2penalty, Float momentum, Float decayFactor, size_t decayAfterEpochs, bool horizontalFlip, bool verticalFlip, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNAddLearningRateSGDR(bool clear, size_t gotoEpoch, Float maximumRate, size_t bachSize, size_t cycles, size_t epochs, size_t epochMultiplier, Float minimumRate, Float L2penalty, Float momentum, Float decayFactor, size_t decayAfterEpochs, bool horizontalFlip, bool verticalFlip, Float dropout, Float cutout, Float autoAugment, Float colorCast, size_t colorAngle, Float distortion, size_t interpolation, Float scaling, Float rotation);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNLoadDataset();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNTraining();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNStop();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNPause();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNResume();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNTesting();

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetTrainingInfo(ref size_t currentCycle, ref size_t totalCycles, ref size_t currentEpoch, ref size_t totalEpochs, ref bool verticalMirror, ref bool horizontalMirror, ref Float dropout, ref Float cutout, ref Float autoAugment, ref Float colorCast, ref size_t colorAngle, ref Float distortion, ref size_t interpolation, ref Float scaling, ref Float rotation, ref size_t sampleIndex, ref size_t batchSize, ref Float rate, ref Float momentum, ref Float l2Penalty, ref Float avgTrainLoss, ref Float trainErrorPercentage, ref size_t trainErrors, ref Float avgTestLoss, ref Float testErrorPercentage, ref size_t testErrors, ref DNNStates networkState, ref DNNTaskStates taskState);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetTestingInfo(ref size_t batchSize, ref size_t sampleIndex, ref Float avgTestLoss, ref Float testErrorPercentage, ref size_t testErrors, ref DNNStates networkState, ref DNNTaskStates taskState);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetNetworkInfo(ref string name, ref size_t costIndex, ref size_t costLayerCount, ref size_t groupIndex, ref size_t labelindex, ref size_t hierarchies, ref bool meanStdNormalization, ref DNNCosts lossFunction, ref DNNDatasets dataset, ref size_t layerCount, ref size_t trainingSamples, ref size_t testingSamples, Float[] meanTrainSet, Float[] stdTrainSet);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNSetOptimizer(DNNOptimizers strategy);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNRefreshStatistics(size_t layerIndex, ref string description, ref Float neuronsStdDev, ref Float neuronsMean, ref Float neuronsMin, ref Float neuronsMax, ref Float weightsStdDev, ref Float weightsMean, ref Float weightsMin, ref Float weightsMax, ref Float biasesStdDev, ref Float biasesMean, ref Float biasesMin, ref Float biasesMax, ref Float fpropLayerTime, ref Float bpropLayerTime, ref Float updateLayerTime, ref Float fpropTime, ref Float bpropTime, ref Float updateTime, ref bool locked);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNGetInputSnapShot([Out] Float[] snapshot, [Out] size_t[] label);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool DNNCheckDefinition([MarshalAs(UnmanagedType.LPWStr)][In, Out] ref string definition, [In,Out] ref DNNCheckMsg checkMsg);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNLoadDefinition([MarshalAs(UnmanagedType.LPWStr)][In] ref string fileName, [In] DNNOptimizers optimizer, [In, Out] ref DNNCheckMsg checkMsg);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNReadDefinition([MarshalAs(UnmanagedType.LPWStr)][In] ref string definition, [In] DNNOptimizers optimizer, [In, Out] ref DNNCheckMsg checkMsg);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNDataprovider([MarshalAs(UnmanagedType.LPWStr)][In] string directory);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNLoadNetworkWeights([MarshalAs(UnmanagedType.LPWStr)] [In] ref string fileName, [In] bool persistOptimizer);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNSaveNetworkWeights([MarshalAs(UnmanagedType.LPWStr)][In] ref string fileName, [In] bool persistOptimizer);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNLoadLayerWeights([MarshalAs(UnmanagedType.LPWStr)][In] ref string fileName, size_t layerIndex, bool persistOptimizer);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		public static extern int DNNSaveLayerWeights([MarshalAs(UnmanagedType.LPWStr)][In] ref string fileName, size_t layerIndex, bool persistOptimizer);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetLayerWeights([In] size_t layerIndex, [Out] Float[] weights, [Out] Float[] biases);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNSetCostIndex(size_t index);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetCostInfo(size_t costIndex, ref size_t trainErrors, ref Float trainLoss, ref Float avgTrainLoss, ref Float trainErrorPercentage, ref size_t testErrors, ref Float testLoss, ref Float avgTestLoss, ref Float testErrorPercentage);

		[DllImport("dnn.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DNNGetImage([In] size_t layer, [In] Byte fillColor, [Out] Byte[] image);
	}


	[Serializable()]
	public class LayerInformation : INotifyPropertyChanged
	{
		[field: NonSerialized()]
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) { PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName)); }

		private bool? lockUpdate;


		public string Name;
		public string Description;
		public DNNLayerTypes LayerType;
		public DNNActivations ActivationFunctionEnum;
		public DNNCosts CostFunction;
		public List<size_t> Inputs;
		public List<string> InputsNames;
		//public System.Windows.Media.Imaging.BitmapSource WeightsSnapshot;
		public bool Lockable;
		public bool? LockUpdate
		{
			get { return lockUpdate; }
			set
			{
				if (value.Equals(lockUpdate))
					return;

				lockUpdate = value;
				OnPropertyChanged("LockUpdate");
			}
		}
		public bool HasDropout;
		public bool HasWeights;
		public bool IsNormalizationLayer;
		public bool HasBias;
		public bool MirrorPad;
		public bool RandomCrop;
		public bool Scaling;
		public bool AcrossChannels;
		public int WeightsSnapshotX;
		public int WeightsSnapshotY;
		public size_t InputCount;
		public size_t LayerIndex;
		public size_t NeuronCount;
		public size_t C;
		public size_t D;
		public size_t W;
		public size_t H;
		public size_t HW;
		public size_t KernelH;
		public size_t KernelW;
		public size_t KernelHW;
		public size_t DilationH;
		public size_t DilationW;
		public size_t StrideH;
		public size_t StrideW;
		public size_t PadD;
		public size_t PadH;
		public size_t PadW;
		public size_t Multiplier;
		public size_t Groups;
		public size_t Group;
		public size_t LocalSize;
		public size_t WeightCount;
		public size_t BiasCount;
		public size_t GroupSize;
		public size_t InputC;
		public size_t GroupIndex;
		public size_t LabelIndex;
		public Float Dropout;
		public Float Cutout;
		public Float NeuronsStdDev;
		public Float NeuronsMean;
		public Float NeuronsMin;
		public Float NeuronsMax;
		public Float WeightsStdDev;
		public Float WeightsMean;
		public Float WeightsMin;
		public Float WeightsMax;
		public Float BiasesStdDev;
		public Float BiasesMean;
		public Float BiasesMin;
		public Float BiasesMax;
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
	}

	public class Model
	{
		public delegate void TrainProgressEventDelegate(size_t BatchSize, size_t Cycle, size_t TotalCycles, size_t Epoch, size_t TotalEpochs, bool HorizontalFlip, bool VerticalFlip, Float Dropout, Float Cutout, Float AutoAugment, Float ColorCast, size_t ColorRadius, Float Distortion, size_t Interpolation, Float Scaling, Float Rotation, size_t SampleIndex, Float Rate, Float Momentum, Float L2Penalty, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, size_t TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, size_t TestErrors, DNNStates State, DNNTaskStates TaskState);
		public delegate void TestProgressEventDelegate(size_t BatchSize, size_t SampleIndex, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, size_t TestErrors, DNNStates State, DNNTaskStates TaskState);
		public delegate void NewEpochEventDelegate(size_t Cycle, size_t Epoch, size_t TotalEpochs, bool HorizontalFlip, bool VerticalFlip, Float Dropout, Float Cutout, Float AutoAugment, Float ColorCast, size_t ColorAngle, Float Distortion, size_t Interpolation, Float Scaling, Float Rotation, Float Rate, size_t BatchSize, Float Momentum, Float L2Penalty, Float AvgTrainLoss, Float TrainErrorPercentage, Float TrainAccuracy, size_t TrainErrors, Float AvgTestLoss, Float TestErrorPercentage, Float TestAccuracy, size_t TestErrors);

		public TrainProgressEventDelegate TrainProgress;
		public TestProgressEventDelegate TestProgress;
		public NewEpochEventDelegate NewEpoch;

		public int SelectedIndex;
		public System.Collections.ObjectModel.ObservableCollection<LayerInformation> Layers;
		//public System.Windows.Media.Imaging.BitmapSource InputSnapshot;
		public string Label;
		public List<DNNCostLayer> CostLayers;
		public List<Float> MeanTrainSet;
		public List<Float> StdTrainSet;
		public List<List<size_t>> ConfusionMatrix;
		public List<List<string>> LabelsCollection;
		public List<DNNTrainingRate> TrainingRates;
		public DNNTrainingRate TrainingRate;
		public string DefinitionDocument;
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
		public size_t CostIndex;
		public size_t Hierarchies;
		public size_t ClassCount;
		public size_t GroupIndex;
		public size_t LabelIndex;
		public size_t BatchSize;
		public size_t LayerCount;
		public size_t Multiplier;
		public size_t CostLayersCount;
		public size_t TrainingSamples;
		public size_t AdjustedTrainingSamplesCount;
		public size_t TestingSamples;
		public size_t AdjustedTestingSamplesCount;
		public size_t Cycle;
		public size_t TotalCycles;
		public size_t Epoch;
		public size_t TotalEpochs;
		public Float ColorCast;
		public size_t ColorAngle;
		public size_t Interpolation;
		public size_t SampleIndex;
		public size_t TrainErrors;
		public size_t TestErrors;
		public size_t BlockSize;
		public Float Dropout;
		public Float Cutout;
		public Float AutoAugment;
		public Float Distortion;
		public Float Scaling;
		public Float Rotation;
		public Float AdaDeltaEps;
		public Float AdaGradEps;
		public Float AdamEps;
		public Float AdamBeta2;
		public Float AdamaxEps;
		public Float AdamaxBeta2;
		public Float RMSPropEps;
		public Float RAdamEps;
		public Float RAdamBeta1;
		public Float RAdamBeta2;
		public Float AvgTrainLoss;
		public Float TrainErrorPercentage;
		public Float AvgTestLoss;
		public Float TestErrorPercentage;
		public Float Rate;
		public Float Momentum;
		public Float L2Penalty;
		public Float AvgSampleRate;
		public DNNStates State;
		public DNNTaskStates TaskState;
		public Float fpropTime;
		public Float bpropTime;
		public Float updateTime;
		public bool PersistOptimizer;
		public bool DisableLocking;

		public Model(string name, string fileName, DNNOptimizers optimizer)
        {
			Name = name;
			Optimizer = optimizer;
						
			Duration = new Stopwatch();
			SampleSpeedTimer = new Stopwatch();

			sb = new StringBuilder();
			State = DNNStates.Idle;
			OldState = DNNStates.Idle;
			TaskState = DNNTaskStates.Stopped;
			MeanTrainSet = new List<Float>(3);
			StdTrainSet = new List<Float>(3);
			StorageDirectory = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\convnet\\");
			DatasetsDirectory = string.Concat(StorageDirectory, "datasets\\");
			DefinitionsDirectory = string.Concat(StorageDirectory, "definitions\\");

			AdaGradEps = 1e-08f;
			AdamEps = 1e-08f;
			AdamBeta2 = 0.999f;
			AdamaxEps = 1e-08f;
			AdamaxBeta2 = 0.999f;
			RAdamEps = 1e-08f;
			RAdamBeta1 = 0.9f;
			RAdamBeta2 = 0.999f;
			RMSPropEps = 1e-08f;

			GroupIndex = 0;
			LabelIndex = 0;
			CostIndex = 0;
			Multiplier = 1;

			NativeMethods.DNNDataprovider(StorageDirectory);

            var checkMsg = new DNNCheckMsg();
            if (NativeMethods.DNNLoadDefinition(ref fileName, optimizer, ref checkMsg) == 1)
            {
                Optimizer = optimizer;

                NativeMethods.DNNLoadDataset();

                StreamReader reader = new StreamReader(fileName, true);
                DefinitionDocument = reader.ReadToEnd();
                reader.Close();

                ApplyParameters();

                WorkerTimer = new System.Timers.Timer(1000.0);
                WorkerTimer.Elapsed += new ElapsedEventHandler(OnElapsed);
            }
            else
            {
                throw new Exception(checkMsg.Message);
            }
        }

		~Model() 
		{ 
		}

		public bool LoadDataset()
        {
			return false;
        }

		public List<string> GetTextLabels(string fileName)
        {
			List<string> labels = new List<string>();

			return labels;
		}

		public void OnElapsed(Object sender, ElapsedEventArgs e)
        {

        }

		public void UpdateInputSbapshot(size_t C, size_t H, size_t W)
        {

        }

		public void SetPersistOptimizer(bool persist)
        {

        }

		public void SetDisableLocking(bool disable)
        {

        }

		public void SetOptimizersHyperParameters(Float adaDeltaEps, Float adaGradEps, Float adamEps, Float adamBeta2, Float adamaxEps, Float adamaxBeta2, Float rmsPropEps, Float radamEps, Float radamBeta1, Float radamBeta2)
        {

        }

		public void ApplyParameters()
        {

        }
		public void ResetLayerWeights(size_t layerIndex)
        {

        }

		public void ResetWeights()
        {

        }

		public void AddLearningRate(bool clear, size_t gotoEpoch, DNNTrainingRate rate)
        {

        }

		public void AddLearningRateSGDR(bool clear, size_t gotoEpoch, DNNTrainingRate rate)
        {

        }

		public void Start(bool training)
        {

        }

		public void Stop()
        {

        }

		public void Pause()
        {

        }

		public void Resume()
        {

        }

		public void UpdateLayerStatistics(LayerInformation info, size_t layerIndex, bool updateUI)
        {

        }

		public LayerInformation GetLayerInfo(size_t layerIndex, bool updateUI)
        {
			return null;
        }

		public void UpdateLayerInfo(size_t layerIndex, bool updateUI)
        {

        }

		public void SetCostIndex(size_t costIndex)
        {

        }

		public void SetOptimizer(DNNOptimizers optimizer)
        {

        }

		public void SetLocked(bool locked)
        {

        }

		public void SetLayerLocked(size_t layerIndex, bool locked)
        {

        }

		public void GetConfusionMatrix()
        {

        }

		public void UpdateCostInfo(size_t costIndex)
        {

        }

		public bool BatchNormalizationUsed()
        {
			return true;
        }

		public DNNCheckMsg CheckDefinition(string definition)
        {
			return new DNNCheckMsg();
		}

		public int LoadDefinition(string fileName)
        {
			return 0;
        }

		public int LoadWeights(string fileName, bool persistOptimizer)
		{
			return 0;
		}

		public int SaveWeights(string fileName, bool persistOptimizer)
		{
			return 0;
		}

		public int LoadLayerWeights(string fileName, size_t layerIndex)
		{
			return 0;
		}

		public int SaveLayerWeights(string fileName, size_t layerIndex)
		{
			return 0;
		}

		public static bool StochasticEnabled()
		{
			return false;
		}

		protected System.Timers.Timer WorkerTimer;
		protected Stopwatch SampleSpeedTimer;
		protected TimeSpan OptimizeTime;
		protected StringBuilder sb;
		protected string oldWeightSaveFileName;
		protected DNNStates OldState;
	};
	
	class Program
    {
		public static string ApplicationPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";
		public static string StorageDirectory { get; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\convnet\";
		public static string StateDirectory { get; } = StorageDirectory + @"state\";
		public static string DefinitionsDirectory { get; } = StorageDirectory + @"definitions\";
		public static string ScriptsDirectory { get; } = StorageDirectory + @"scripts\";

		static void Main(string[] args)
        {
			Model model = new Model("resnet-32-4-3-2-6-dropout-channelzeropad", Path.Combine(StateDirectory, "resnet-32-4-3-2-6-dropout-channelzeropad.definition"), DNNOptimizers.NAG);
		}
    }
}
