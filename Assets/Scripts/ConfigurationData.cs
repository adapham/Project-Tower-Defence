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

        // configuration data with default values
        static float _spawnDelay;
        static int _maxEnemiesInScene;
        static int _maxLives;
        static int _totalEnemy;

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

        #endregion
    }
}
