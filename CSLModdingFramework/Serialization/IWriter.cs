namespace CSLModdingFramework.Serialization;

public interface IWriter {
    void StartWrite(string fileName, string sectionName, SectionWriteMode sectionWriteMode = SectionWriteMode.Replace);
    void WriteProperty<T>(string name, T value);
    void RemoveSection(string sectionName);
    void RemoveOtherSections();
    void EndWrite();
}