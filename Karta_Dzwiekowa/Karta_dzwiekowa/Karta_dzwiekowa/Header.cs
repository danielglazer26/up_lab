using System;
using System.IO;

namespace Karta_dzwiekowa
{
    //Klasa odpowiadająca za odczytanie i wyświetlanie nagłówka
    class Header
    {
        short formattype; //typ formatu
        short channels; //liczba kanałów
        int samplespersec; //częstotliwość próbkowania
        short bytespersample; //bajty na próbkę
        short bitespersample; //bity na próbkę
        public Header(String sciezka)
        {
            //użycie FileStream oraz BinaryReader do odczytu wartości z nagłówka
            using (var fileStream = new FileStream(sciezka, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                try
                {
                    convertBinary(binaryReader.ReadBytes(4));
                    BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    convertBinary(binaryReader.ReadBytes(4));
                    convertBinary(binaryReader.ReadBytes(4));
                    BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    formattype = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    channels = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    samplespersec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    bytespersample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    bitespersample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                }
                finally
                {
                    binaryReader.Close(); //zamknięcie binaryReader
                    fileStream.Close(); //zamknięcie strumienia danych
                }

            }
        }
        //konwersja na łańcuch znakowy
        private string convertBinary(Byte[] bytes)
        {
            String word = "";
            foreach (var item in bytes)
            {
                word += (char)item;
            }

            return word;
        }

        public short getFormat()
        {
            return formattype;
        }
        public short getChannels()
        {
            return channels;
        }
        public int getSamplespersec()
        {
            return samplespersec;
        }
        public short getBytespersample()
        {
            return bytespersample;
        }
        public short getBitespersample()
        {
            return bitespersample;
        }

        //wyświetlenie nagłówka w konsoli
        public void ReadHeader(string sciezka)
        {
            using (var fileStream = new FileStream(sciezka, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                try
                {
                    Console.WriteLine("RIFF: " + convertBinary(binaryReader.ReadBytes(4)));
                    Console.WriteLine("SIZE: " + BitConverter.ToInt32(binaryReader.ReadBytes(4), 0));
                    Console.WriteLine("WAVE: " + convertBinary(binaryReader.ReadBytes(4)));
                    Console.WriteLine("fmt: " + convertBinary(binaryReader.ReadBytes(4)));
                    Console.WriteLine("Rozmiar formatu: " + BitConverter.ToInt32(binaryReader.ReadBytes(4), 0));
                    formattype = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    Console.WriteLine("Typ formatu: " + formattype);
                    channels = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    Console.WriteLine("Liczba kanałów: " + channels);
                    samplespersec = BitConverter.ToInt32(binaryReader.ReadBytes(4), 0);
                    Console.WriteLine("SampleRate: " + samplespersec);
                    Console.WriteLine("BytesPerSecound " + BitConverter.ToInt32(binaryReader.ReadBytes(4), 0));
                    bytespersample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    Console.WriteLine("BytesPerSample: " + bytespersample);
                    bitespersample = BitConverter.ToInt16(binaryReader.ReadBytes(2), 0);
                    Console.WriteLine("BitsPerSample: " + bitespersample);
                    Console.WriteLine("Data: " + convertBinary(binaryReader.ReadBytes(4)));
                    Console.WriteLine("DataSize: " + BitConverter.ToInt32((binaryReader.ReadBytes(4)), 0));

                }
                finally
                {
                    binaryReader.Close();
                    fileStream.Close();
                }

            }
        }


    }

}
