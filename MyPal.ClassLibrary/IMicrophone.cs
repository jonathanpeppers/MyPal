namespace MyPal.ClassLibrary;

public interface IMicrophone
{
    void Start();

    Stream GetAudio();
}
