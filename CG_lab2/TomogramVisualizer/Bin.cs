using System.IO;

public class Bin
{
    public static int X, Y, Z;
    public static short[] array;

    public Bin() { }

    public void readBIN(string path)
    {
        if (File.Exists(path))
        {
            BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open));

            X = reader.ReadInt32();
            Y = reader.ReadInt32();
            Z = reader.ReadInt32();

            int arraySize = X * Y * Z;
            short[] originalArray = new short[arraySize];
            array = new short[arraySize];

            for (int i = 0; i < arraySize; ++i)
            {
                //array[i] = reader.ReadInt16();    //раскомментить строку для изначального отображения, блоки с транспонированием и свопом переменных закомментить

                originalArray[i] = reader.ReadInt16();
            }

            //Вид сбоку. Транспонируем данные, меняя X и Z местами
            for (int z = 0; z < Z; z++)
            {
                for (int y = 0; y < Y; y++)
                {
                    for (int x = 0; x < X; x++)
                    {
                        // Оригинальный индекс: x + y * X + z * X * Y
                        // Новый индекс: z + y * Z + x * Z * Y
                        array[z + y * Z + x * Z * Y] = originalArray[x + y * X + z * X * Y];
                    }
                }
            }

            int temp = X;
            X = Z;
            Z = temp;

            ////Вид спереди. Транспонируем данные, меняя Y и Z местами
            //array = new short[arraySize];
            //for (int z = 0; z < Z; z++)
            //{
            //    for (int y = 0; y < Y; y++)
            //    {
            //        for (int x = 0; x < X; x++)
            //        {
            //            // Оригинальный индекс: x + y * X + z * X * Y
            //            // Новый индекс: x + z * X + y * X * Z
            //            array[x + z * X + y * X * Z] = originalArray[x + y * X + z * X * Y];
            //        }
            //    }
            //}

            //int temp = Y;
            //Y = Z;
            //Z = temp;
        }
    }
}