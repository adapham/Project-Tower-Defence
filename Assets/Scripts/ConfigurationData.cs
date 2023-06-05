using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Assets.Scripts
{
    public class ConfigurationData
    {
        #region Fields

        const string ConfigurationDataFileName = "ConfigurationData.csv";
        const string ResultDataFileName = "D:\\FU_SUMMER2023\\PRU221m\\ProjectTowerDefense\\Project-Tower-Defence\\Assets\\StreamingAssets\\ResultData.csv";

        // configuration data with default values
        static float _spawnDelay;
        static int _maxEnemiesInScene;
        static int _maxLives;
        static int _totalEnemy;
        public int _resultPoint;

        #endregion

        #region Properties

        public float SpawnDelay
        {
            get
            {
                return _spawnDelay;
            }
        }
        public int MaxEnemiesInScene
        {
            get
            {
                return _maxEnemiesInScene;
            }
        }
        public int MaxLives
        {
            get
            {
                return _maxLives;
            }
        }
        public int TotalEnemy
        {
            get
            {
                return _totalEnemy;
            }
        }

        public int ResultPoint
        {
            get { return _resultPoint; }
            set { _resultPoint = value; }
        }

        #endregion

        #region Constructor
        public ConfigurationData()
        {
            // read and save configuration data from file
            StreamReader input = null;
            try
            {
                // create stream reader object
                input = File.OpenText(Path.Combine(
                    Application.streamingAssetsPath, ConfigurationDataFileName));

                // read in names and values
                string names = input.ReadLine();
                string values = input.ReadLine();

                // set configuration data fields
                SetConfigurationDataFields(values);

            }
            catch (Exception e)
            {
                Debug.LogError("Can't read file ConfigurationData" + e.Message);
            }
            finally
            {
                // always close input file
                if (input != null)
                {
                    input.Close();
                }
            }
        }

        static void SetConfigurationDataFields(string csvValues)
        {
            string[] values = csvValues.Split(',');
            _spawnDelay = float.Parse(values[0]);
            _maxEnemiesInScene = int.Parse(values[1]);
            _maxLives = int.Parse(values[2]);
            _totalEnemy = int.Parse(values[3]);
        }

        public void SaveToFile(int content)
        {
            StreamWriter output = null;
            try
            {
                // create stream writer object
                output = new StreamWriter(ResultDataFileName);

                Debug.Log("Content: " + content);
                // write the content to the file
                output.WriteLine("ResultPoint");
                output.WriteLine(content.ToString());

                // flush the output buffer and close the file
                output.Flush();
                output.Close();
            }
            catch (IOException e)
            {
                Debug.LogError("Error saving file: " + e.Message);
            }
            finally
            {
                // always close output file
                if (output != null)
                {
                    output.Close();
                }
            }
        }
        #endregion

    }
}
