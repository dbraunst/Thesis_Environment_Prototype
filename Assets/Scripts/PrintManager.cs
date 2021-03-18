using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;

using Debug = UnityEngine.Debug;

public class PrintManager : MonoBehaviour
{
    static StreamWriter writer;

    public string path = "Assets/Resources/test01.txt";

    //TODO? Implement this if maybe you feel like it
    // public bool append = false;
    // Start is called before the first frame update
    void Start()
    {
        path = checkFile(path);
        //write text to new text file
        writer = new StreamWriter(path, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Entah"); 
            writeTestString();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Pressed P, added line \'This is a new line\' to output file");
            writer.WriteLine("This is a new line");
        }
    }

    void OnDestroy()
    {
        if (writer.BaseStream != null)
        {
            writer.Close();
        }
    }

    [UnityEditor.MenuItem("Text Output/Write File")]
    static void writeTestString() {

        writer.WriteLine("You pressed enter, and now here's the file!");
        writer.Close();
    }

     static string checkFile(string path) {
        // See if file Currently exists at path
        if (File.Exists(path)){

            string[] _file = path.Split('.');
            // Debug.Log("File Name: " + _file[0] + ", File Type: ." + _file[1]);

            // See if file has existing version number - e.g. "myfile_01.txt"
            string pattern = @"\d+$";
            Match m = Regex.Match(_file[0], pattern, RegexOptions.None);
            if (m.Success) {

                int fileNum = int.Parse(m.Value);

                // Set incremented file number int "_##" 
                string fileNum_s = ++fileNum < 9 ? "_0" + fileNum.ToString() : "_" + fileNum.ToString();

                // Trim old file number, add in path, recombine with split filetype
                path = _file[0].Remove(m.Index).Insert(m.Index, fileNum_s) + "." + _file[1];

            } else {
                
                // File not numbered, add "_01" to the end of name and begin increment
                path = _file[0] + "_01." + _file[1];
            }

            //recursive call just in case you said do _01 but have up to _18 already.
            return checkFile(path);
        } 
        else { //File Does not Currently Exist
            return path;
        }
    }

    public void AddLine(string _msg) {
        writer.WriteLine(_msg);
    }
}
