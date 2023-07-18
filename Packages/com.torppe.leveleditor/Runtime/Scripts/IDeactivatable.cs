public interface IDeactivatable
{
    bool Deactivated { get; set; }
    void Deactivate();
}
