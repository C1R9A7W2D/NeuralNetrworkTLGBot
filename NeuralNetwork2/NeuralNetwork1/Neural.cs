using System;
using System.Collections;
using System.Collections.Generic;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class Sample
    {
        public double[] input = null;
        public double[] error = null;
        public ZodiacSign actualClass;
        public ZodiacSign recognizedClass;

        public Sample(double[] inputValues, int classesCount, ZodiacSign sampleClass = ZodiacSign.Undef)
        {
            input = (double[])inputValues.Clone();
            Output = new double[classesCount];
            if (sampleClass != ZodiacSign.Undef) Output[(int)sampleClass] = 1;

            recognizedClass = ZodiacSign.Undef;
            actualClass = sampleClass;
        }

        public double[] Output { get; private set; }

        public ZodiacSign ProcessPrediction(double[] neuralOutput)
        {
            Output = neuralOutput;
            if (error == null)
                error = new double[Output.Length];

            recognizedClass = 0;
            for (int i = 0; i < Output.Length; ++i)
            {
                error[i] = (Output[i] - (i == (int)actualClass ? 1 : 0));
                if (Output[i] > Output[(int)recognizedClass])
                    recognizedClass = (ZodiacSign)i;
            }

            return recognizedClass;
        }

        public double EstimatedError()
        {
            double Result = 0;
            for (int i = 0; i < Output.Length; ++i)
                Result += Math.Pow(error[i], 2);
            return Result;
        }

        public void updateErrorVector(double[] errorVector)
        {
            for (int i = 0; i < errorVector.Length; ++i)
                errorVector[i] += error[i];
        }

        public override string ToString()
        {
            string result = $"Sample: {actualClass} ({(int)actualClass})\n";
            result += $"Recognized: {recognizedClass} ({(int)recognizedClass})";

            if (!Correct())
                result += " [INCORRECT]";

            return result;
        }

        public bool Correct()
        {
            return actualClass == recognizedClass;
        }
    }

    public class SamplesSet : IEnumerable
    {
        public List<Sample> samples = new List<Sample>();

        public void AddSample(Sample image)
        {
            samples.Add(image);
        }

        public int Count => samples.Count;

        public IEnumerator GetEnumerator()
        {
            return samples.GetEnumerator();
        }

        public Sample this[int i]
        {
            get => samples[i];
            set => samples[i] = value;
        }

        public double TestNeuralNetwork(BaseNetwork network)
        {
            double correct = 0;
            double wrong = 0;
            foreach (var sample in samples)
            {
                if (sample.actualClass == network.Predict(sample))
                    ++correct;
                else
                    ++wrong;
            }
            return correct / (correct + wrong);
        }
    }
}