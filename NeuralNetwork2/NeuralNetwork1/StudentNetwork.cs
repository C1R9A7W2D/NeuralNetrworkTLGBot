using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    public class StudentNetwork : BaseNetwork
    {
        private readonly int[] structure;
        private double[][,] weights;
        private double[][] biases;
        private double[][] layerOutputs;
        private double[][] layerInputs;
        private double[][] deltas;

        private Random random = new Random();
        private Stopwatch stopWatch = new Stopwatch();

        public StudentNetwork(int[] structure)
        {
            this.structure = structure;
            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            int layerCount = structure.Length - 1;

            weights = new double[layerCount][,];
            biases = new double[layerCount][];
            layerOutputs = new double[layerCount + 1][];
            layerInputs = new double[layerCount][];
            deltas = new double[layerCount][];

            for (int layer = 0; layer < layerCount; layer++)
            {
                int inputSize = structure[layer];
                int outputSize = structure[layer + 1];

                double std = Math.Sqrt(2.0 / (inputSize + outputSize));
                weights[layer] = new double[inputSize, outputSize];
                for (int i = 0; i < inputSize; i++)
                    for (int j = 0; j < outputSize; j++)
                        weights[layer][i, j] = (random.NextDouble() - 0.5) * 2.0 * std;

                biases[layer] = new double[outputSize];
                for (int j = 0; j < outputSize; j++)
                    biases[layer][j] = (random.NextDouble() - 0.5) * 0.1;

                layerInputs[layer] = new double[outputSize];
                deltas[layer] = new double[outputSize];
            }

            for (int i = 0; i <= layerCount; i++)
                layerOutputs[i] = new double[structure[i]];
        }

        private double Sigmoid(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        private double SigmoidDerivative(double x)
        {
            double s = Sigmoid(x);
            return s * (1 - s);
        }

        protected override double[] Compute(double[] input)
        {
            Array.Copy(input, layerOutputs[0], input.Length);

            for (int layer = 0; layer < structure.Length - 1; layer++)
            {
                int inputSize = structure[layer];
                int outputSize = structure[layer + 1];

                for (int to = 0; to < outputSize; to++)
                {
                    double sum = biases[layer][to];

                    for (int from = 0; from < inputSize; from++)
                        sum += layerOutputs[layer][from] * weights[layer][from, to];

                    layerInputs[layer][to] = sum;
                    layerOutputs[layer + 1][to] = Sigmoid(sum);
                }
            }

            return layerOutputs[layerOutputs.Length - 1];
        }

        public override int Train(Sample sample, double acceptableError, bool parallel)
        {
            int iterations = 0;
            double error;

            do
            {
                Compute(sample.input);
                Backward(sample.Output);
                UpdateWeights(0.1);

                error = 0;
                for (int i = 0; i < sample.Output.Length; i++)
                    error += Math.Pow(layerOutputs[layerOutputs.Length - 1][i] - sample.Output[i], 2);

                iterations++;
            } while (error > acceptableError && iterations < 1000);

            return iterations;
        }

        private void Backward(double[] target)
        {
            int outputLayer = structure.Length - 2;
            double[] output = layerOutputs[outputLayer + 1];

            for (int i = 0; i < structure[outputLayer + 1]; i++)
                deltas[outputLayer][i] = (output[i] - target[i]) * SigmoidDerivative(layerInputs[outputLayer][i]);

            for (int layer = outputLayer - 1; layer >= 0; layer--)
            {
                for (int i = 0; i < structure[layer + 1]; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < structure[layer + 2]; j++)
                        sum += deltas[layer + 1][j] * weights[layer + 1][i, j];

                    deltas[layer][i] = sum * SigmoidDerivative(layerInputs[layer][i]);
                }
            }
        }

        private void UpdateWeights(double learningRate)
        {
            for (int layer = 0; layer < structure.Length - 1; layer++)
            {
                for (int from = 0; from < structure[layer]; from++)
                {
                    for (int to = 0; to < structure[layer + 1]; to++)
                    {
                        weights[layer][from, to] -= learningRate * deltas[layer][to] * layerOutputs[layer][from];
                    }
                }

                for (int to = 0; to < structure[layer + 1]; to++)
                    biases[layer][to] -= learningRate * deltas[layer][to];
            }
        }

        public override double TrainOnDataSet(SamplesSet samplesSet, int epochsCount, double acceptableError, bool parallel)
        {
            stopWatch.Restart();
            double bestError = double.MaxValue;
            List<Sample> samples = samplesSet.samples.ToList();

            for (int epoch = 0; epoch < epochsCount; epoch++)
            {
                samples = samples.OrderBy(x => random.Next()).ToList();

                double epochError = 0;
                int correct = 0;

                foreach (var sample in samples)
                {
                    double[] output = Compute(sample.input);
                    Backward(sample.Output);
                    UpdateWeights(0.1);

                    for (int i = 0; i < output.Length; i++)
                        epochError += Math.Pow(output[i] - sample.Output[i], 2);

                    int predicted = 0;
                    double max = output[0];
                    for (int i = 1; i < output.Length; i++)
                        if (output[i] > max)
                        {
                            max = output[i];
                            predicted = i;
                        }

                    if (predicted == (int)sample.actualClass)
                        correct++;
                }

                epochError /= samples.Count;
                double accuracy = (double)correct / samples.Count;

                if (epochError < bestError)
                    bestError = epochError;

                double progress = (epoch + 1.0) / epochsCount;
                OnTrainProgress(progress, epochError, stopWatch.Elapsed);

                if (epoch % 10 == 0)
                    Console.WriteLine($"Epoch {epoch}: Error={epochError:F6}, Accuracy={accuracy:P2}");

                if (epochError <= acceptableError)
                    break;
            }

            stopWatch.Stop();
            OnTrainProgress(1.0, bestError, stopWatch.Elapsed);

            return bestError;
        }
    }
}