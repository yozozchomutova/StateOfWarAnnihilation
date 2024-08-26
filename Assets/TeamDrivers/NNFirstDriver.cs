using System;
using System.Collections;
using System.Numerics;
using System.Runtime.InteropServices;
using UnityEngine;

public class NNFirstDriver : TeamDriver
{
    public MLP nnUnitManager;

    public const int INPUT_SIZE = 128 * 128;
    public const int HIDDEN_SIZE = 512;
    public const int OUTPUT_SIZE = 4;

    public override void onInit()
    {

    }

    public override void onUpdate()
    {
        if (LevelData.teamStats[controllingTeam.id].activeTeam && NNManagerPanel.LAUNCHED)
        {
            //1. Feed the data to neural network
            float[] doubleArray = new float[INPUT_SIZE];
            for (int x = 0; x < NNManagerPanel.mapGroundUnits.GetLength(0); x++)
            {
                for (int y = 0; y < NNManagerPanel.mapGroundUnits.GetLength(1); y++)
                {
                    float element = 0;
                    byte mapGroundUnit = NNManagerPanel.mapGroundUnits[x, y];

                    if (mapGroundUnit == 255)
                    {
                        element = 0;
                    } else if (mapGroundUnit != controllingTeam.id)
                    {
                        element = 1;
                    }

                    element = 2;
                    //element = UnityEngine.Random.Range(0, 10);
                    doubleArray[x * NNManagerPanel.mapGroundUnits.GetLength(0) + y] = element;
                }
            }

            float[] output = nnUnitManager.Forward(doubleArray);

            //2. Launch neural network

            //3. Process output from neural network
            print(gameObject.name + "|S: " + output.Length + " |1: " + doubleArray[0] + " |2 " + doubleArray[1] + " |3 " + doubleArray[2] + " |4 " + doubleArray[3]);
        }
    }

    public class MLP
    {
        private ComputeShader shader;
        private int kernelIndex;
        private ComputeBuffer inputBuffer;
        private ComputeBuffer[] weightsBuffers;
        private ComputeBuffer[] biasBuffers;
        private ComputeBuffer[] outputBuffers;

        private int inputSize = 128 * 128;
        private int hiddenSize = 512;
        private int outputSize = 4;

        private int[] layerSizes;

        public MLP(int inputSize, int hiddenSize, int outputSize, ComputeShader cs)
        {
            shader = cs;
            kernelIndex = shader.FindKernel("CSMain");

            layerSizes = new int[] { inputSize, hiddenSize, outputSize };

            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            inputBuffer = new ComputeBuffer(inputSize, sizeof(float));
            weightsBuffers = new ComputeBuffer[layerSizes.Length - 1];
            biasBuffers = new ComputeBuffer[layerSizes.Length - 1];
            outputBuffers = new ComputeBuffer[layerSizes.Length];

            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                weightsBuffers[i] = new ComputeBuffer(layerSizes[i + 1] * layerSizes[i], sizeof(float));
                biasBuffers[i] = new ComputeBuffer(layerSizes[i + 1], sizeof(float));
            }

            for (int i = 0; i < layerSizes.Length; i++)
            {
                outputBuffers[i] = new ComputeBuffer(layerSizes[i], sizeof(float));
            }
        }

        public void Dispose()
        {
            inputBuffer.Dispose();

            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                weightsBuffers[i].Dispose();
                biasBuffers[i].Dispose();
            }

            for (int i = 0; i < layerSizes.Length; i++)
            {
                outputBuffers[i].Dispose();
            }
        }

        public float[] Forward(float[] input)
        {
            // Set input buffer data
            inputBuffer.SetData(input);

            // Set weights and biases buffers data
            for (int i = 0; i < layerSizes.Length - 1; i++)
            {
                shader.SetBuffer(kernelIndex, $"weights{i}", weightsBuffers[i]);
                shader.SetBuffer(kernelIndex, $"bias{i}", biasBuffers[i]);
            }

            // Set output buffers
            for (int i = 0; i < layerSizes.Length; i++)
            {
                shader.SetBuffer(kernelIndex, $"output{i}", outputBuffers[i]);
            }

            // Set kernel parameters
            shader.SetInt("inputSize", inputSize);
            shader.SetInt("hiddenSize", hiddenSize);
            shader.SetInt("outputSize", outputSize);

            // Dispatch compute shader
            shader.Dispatch(kernelIndex, hiddenSize, 1, 1);

            // Get output data
            float[] output = new float[outputSize];
            outputBuffers[outputBuffers.Length - 1].GetData(output);

            return output;
        }
    }
}
