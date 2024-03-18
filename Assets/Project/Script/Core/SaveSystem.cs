using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public static class SaveSystem
    {
        public static void SaveHighScore(int highScore)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            string path = Application.persistentDataPath + "/Highscore.fun";
            FileStream stream = new FileStream(path, FileMode.Create);

            formatter.Serialize(stream, highScore);
            stream.Close();
        }

        public static int LoadHighScore()
        {
            string path = Application.persistentDataPath + "/Highscore.fun";

            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(path, FileMode.Open);

                int data = Convert.ToInt32(formatter.Deserialize(stream));
                stream.Close();

                return data;
            }
            else
            {
                return 0;
            }
        }
    }
}
