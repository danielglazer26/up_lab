using SlimDX.DirectSound;
using SlimDX.Multimedia;
using System;
using System.IO;
using System.Threading;

namespace Karta_dzwiekowa
{
    class DS
    {
        Thread fillBuffer; //wątek wypełniania SecondarySoundBuffer
        SecondarySoundBuffer sBuffer1; //drugi bufor do odczytywanania danych
        PlayFlags playFlags = PlayFlags.Looping; //ustawienie flagi na looping
        Stream stream; //strumień danych
        public DS(string audioFile, IntPtr window, Header header, bool echo_start)
        {
            
            DirectSound ds = new DirectSound(); //utworzenie obiektu DirectSound

            ds.SetCooperativeLevel(window, CooperativeLevel.Priority); //ustawienie zgodności wątków okna i odtwarzania, ustawienie flagi na priorytet

            WaveFormat format = new WaveFormat(); //utworzenie formatu pliku WAV
            //sczytywanie formatu z nagłówka
            format.BitsPerSample = header.getBitespersample(); 
            format.BlockAlignment = header.getBytespersample();
            format.Channels = header.getChannels();
            format.FormatTag = WaveFormatTag.Pcm;
            format.SamplesPerSecond = header.getSamplespersec();
            format.AverageBytesPerSecond = format.SamplesPerSecond * format.BlockAlignment;

            //utworzenie formatu dla PrimarySoundBuffer
            SoundBufferDescription desc = new SoundBufferDescription();
            desc.Format = format;
            desc.Flags = BufferFlags.GlobalFocus;
            desc.SizeInBytes = 8 * format.AverageBytesPerSecond;

            //utworzenie PrimarySoundBuffer
            PrimarySoundBuffer pBuffer = new PrimarySoundBuffer(ds, desc);

            //utworzenie formatu dla SecondarySoundBuffer
            SoundBufferDescription desc2 = new SoundBufferDescription();
            desc2.Format = format;
            desc2.Flags = BufferFlags.GlobalFocus | BufferFlags.ControlPositionNotify | BufferFlags.GetCurrentPosition2 | BufferFlags.ControlEffects;
            desc2.SizeInBytes = 8 * format.AverageBytesPerSecond;

            //utworzenie SecondarySoundBuffer
            this.sBuffer1 = new SecondarySoundBuffer(ds, desc2);

            // utworzenie tablic dla odczytywania pozycji w pliku WAV
            NotificationPosition[] notifications = new NotificationPosition[2];
            notifications[0].Offset = desc2.SizeInBytes / 2 + 1;
            notifications[1].Offset = desc2.SizeInBytes - 1; ;

            notifications[0].Event = new AutoResetEvent(false);
            notifications[1].Event = new AutoResetEvent(false);
            sBuffer1.SetNotificationPositions(notifications);

            //utworzenie tablic dla danych pliku WAV
            byte[] bytes1 = new byte[desc2.SizeInBytes / 2];
            byte[] bytes2 = new byte[desc2.SizeInBytes];

            //otwarcie strumienia danych
            stream = File.Open(audioFile, FileMode.Open);
            //sprawdzenie czy włączyć efkt echo
            if (echo_start)
            {
                var echo = SoundEffectGuid.StandardEcho; //wybranie efektu z SoundEffectGuid
                var guids = new[] { echo }; //przekazanie danych do tablicy
                sBuffer1.SetEffects(guids); //nałożenie efektu echa na SecondarySoundBuffer

            }
            //wątek wypełniania SecondarySoundBuffer danymi z pliku WAV
            this.fillBuffer = new Thread(() =>
            {
                int bytesRead;
                //odczyt danych
                bytesRead = stream.Read(bytes2, 0, desc2.SizeInBytes);
                //przekaznie do bufora
                sBuffer1.Write<byte>(bytes2, 0, LockFlags.None);
                //włączenie odtwarzania
                sBuffer1.Play(0, playFlags);
                while (true)
                {
                    //sczytywanie danych do buforów
                    if (bytesRead == 0) { break; }
                    notifications[0].Event.WaitOne();
                    bytesRead = stream.Read(bytes1, 0, bytes1.Length);
                    sBuffer1.Write<byte>(bytes1, 0, LockFlags.None);

                    if (bytesRead == 0) { break; }
                    notifications[1].Event.WaitOne();
                    bytesRead = stream.Read(bytes1, 0, bytes1.Length);
                    sBuffer1.Write<byte>(bytes1, desc2.SizeInBytes / 2, LockFlags.None);
                }
                stream.Close(); //zamknięcie strumienia danych
                stream.Dispose();
            });
            fillBuffer.Start(); //start wątku
        }

        //zatrzymanie odtwarzania 
        public void Stop()
        {
            fillBuffer.Abort();
            sBuffer1.Stop();
            stream.Close();
            stream.Dispose();
        }


    }
}
