using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OfficeOpenXml;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
    
}
public class LocalDialog
{
    //Link system function       open file dialog
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    public static bool GetOFN([In, Out] OpenFileName ofn)
    {
        return GetOpenFileName(ofn);
    }

    //Link system function       save as file dialog
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
    public static bool GetSFN([In, Out] OpenFileName ofn)
    {
        return GetSaveFileName(ofn);
    }
}
public class FourQueen : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    const int N = 5; //Four Queens N =5; 8 Queens N=9;
    public int[,] x = new int[N,N];
    public int trials = 10000;//100,1000,10000
    List<int> energy = new List<int>();
    public int alphaN;
    double A = 0;
    int[] arr = new int[N*N];
    List<double> alpha = new List<double>(); //Gain of sigmoid
    double p = 0;

    void Start()
    {
        alpha.Add(0.000000001);
        alpha.Add(0.0001);
        alpha.Add(0.01);
        alpha.Add(0.1);
        alpha.Add(0.5);
        alpha.Add(1);
        alpha.Add(2);
        ////////////////
        int[,,,] w= new int[N, N, N, N];
        CalculateWeight(w);
        double s = 0;
        List<int[,]> X = new List<int[,]>();

        Resetarray(x);
        string str="";

        Dictionary<string, int> dic = new Dictionary<string, int>(); //Use dictionary to store state and sort        
        for(long d =0;d< Math.Pow(2, (N-1)*(N-1)); d++)
        {
            string b = Convert.ToString(d, 2).PadLeft((N-1)*(N-1),'0'); //Covert DEC d to (N-1)*(N-1) bit BIN string
            dic.Add(b, 0);
        }   



        ////////////////////////////////
        
        for (int k = 1; k <= trials; k++)//Update x[,] in order
        {

            for (int i = 1; i <= N-1; i++)
            {
                for(int j = 1; j <= N - 1; j++)
                {
                    for(int n=0;n<=N-1; n++)
                    {
                        for (int m = 0; m <= N - 1; m++)
                        {
                            s += w[n, m, i, j] * x[n, m];
                        }
                    }
                  //  Debug.Log("S:" + s);
                    p = SigmoidFunction(s, alpha[alphaN]);
                   // Debug.Log("p:" + p);
                    double random = UnityEngine.Random.Range(0, 1001);
                    if (random <= p * 1000)
                    {
                        x[i, j] = 1;
                    }
                    else
                    {
                        x[i, j] = 0;
                    }                  
                    s = 0;
                }
            }
            //Convert state to string 
            for (int i = 1; i <= N - 1; i++)
            {
                for (int c = 1; c <= N - 1; c++)
                {
                    str += x[i, c].ToString();
                }
            }
           // Debug.Log("state:" + str);
            //Add state to stack
             if (dic.ContainsKey(str))
                dic[str]++;

             str = "";
                         

        }
        var result = (from pair in dic orderby pair.Value descending select pair).Take(60); //Sort by value
        
        foreach (KeyValuePair<string, int> pair in result)
        {
            Debug.Log("result:"+pair.Key.ToString() + " " + pair.Value.ToString());
        }
        
        saveButton.onClick.AddListener(() =>
           {
               CreatExcel(w,result);
           });
        forrer(1, trials, alpha[alphaN]);//Recursion to calculate A of Boltzmann's Theorial Number 
        A = trials / sum;
        Debug.Log("A2:" + A);
    }

    // Update is called once per frame
    void Update () {
        
    }

    public void CreatExcel( int[,,,]w, IEnumerable<KeyValuePair<string,int>> result)
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = "Excel file(*.xlsx)\0*.xlsx";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//Default path
        openFileName.title = "Output";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetSaveFileName(openFileName))
        {


            string createPath = openFileName.file + ".xlsx";
            FileInfo newFile = new FileInfo(createPath);
            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(createPath);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("table1");// create worksheet for weights
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets.Add("table2");// create worksheet for results
                //worksheet.Column(1).Width = 700;
                worksheet.Cells[1, 2].Value = "x00";
                worksheet.Cells[2, 1].Value = "x00";
                worksheet.Cells[2, 2].Value = w[0, 0, 0, 0];
                for (int i = 1; i <= N - 1; i++)
                {
                    for (int j = 1; j <= N - 1; j++)
                    {                     
                         worksheet.Cells[1, (N-1)*j-(N-3) + i].Value = "x" + j + i;
                         worksheet.Cells[2, (N - 1) * j -(N-3) + i].Value = w[0, 0, j, i];
                    }
                }
                for (int i = 1; i <= N - 1; i++)
                {
                    for (int j = 1; j <= N - 1; j++)
                    {
                         worksheet.Cells[(N - 1) * j- (N - 3) + i, 1].Value = "x" + j + i;
                         worksheet.Cells[(N - 1) * j - (N - 3) + i, 2].Value = w[j, i, 0 ,0];
                        
                    }
                }

                for (int i = 1; i <= N - 1; i++)  // Output weights into excel
                {
                    for (int j = 1; j <= N - 1; j++)
                    {
                        for (int n = 1; n <= N - 1; n++) 
                        {
                            for (int m = 1; m <= N - 1; m++) 
                            {
                                worksheet.Cells[(N - 1) * j - (N - 3) + i, (N - 1) * m - (N - 3) + n].Value = w[j, i, m, n];//"w"+j+i+m+n;
                            }
                        }
                    }
                }
                worksheet2.Cells[1, 1].Value = "State";
                worksheet2.Cells[1, 2].Value = "Occurrence number";
                worksheet2.Cells[1, 3].Value = "Energy";
                worksheet2.Cells[1, 4].Value = "Boltzmann's Theorial Number";
                worksheet2.Cells[1, 5].Value = "α=";
                worksheet2.Cells[1, 6].Value = alpha[alphaN];
                int[,] x = new int[N, N];
                Resetarray(x);
                for (int i = 0; i < result.Count(); i++)
                {
                    worksheet2.Cells[2 + i, 1].Value = result.ElementAt(i).Key;
                    worksheet2.Cells[2 + i, 2].Value = result.ElementAt(i).Value;
                    for(int j=0;j<(N-1)*(N-1);j++)
                    {
                        string xn= result.ElementAt(i).Key.Substring(j,1);
                        int Xn = int.Parse(xn);
                        if ((j+1) % (N - 1) == 0)
                        {
                            x[((j + 1) / (N - 1)) , ((j + 1) % (N - 1)) + (N - 1)] = Xn;
                        }
                        else
                        {
                            x[((j + 1) / (N - 1)) + 1, ((j + 1) % (N - 1))] = Xn;
                        }
                    }
                    int e = E(x);
                    worksheet2.Cells[2 + i, 3].Value = e;
                    Resetarray(x);
                    worksheet2.Cells[2 + i, 4].Value = BoltzmannNumber(A, e, alpha[alphaN]);
                }
                package.Save();//save excel
            }
        }

    }
   
    /// //////////////////////////////////////////////////////

    private double SigmoidFunction(double x, double a)
    {
        return 1.0 / (1.0 + (double)Math.Exp(-a * x));
    }

    private int E(int[,] x)// 4 queen energy function
    {
        
        int e2 = 0;
        for (int i=1;i<=N-1; i++)
        {
            int e = 0;
            for (int j=1;j<=N-1;j++)
            {
                e += x[i,j];
            }
            e = e - 1;
            e2 += e * e;
        }
        int e3 = 0;
        for (int j = 1; j <= N - 1; j++)
        {
            int e = 0;
            for (int i = 1; i <= N - 1; i++)
            {
                e += x[i,j];
            }
            e = e - 1;
            e3 += e * e;
        }
        int E = e2 + e3;
        return E;
    }

    private int E8(int[,] x)// 8 queen energy function
    {

        int e2 = 0;
        for (int i = 1; i <= N - 1; i++)
        {
            int e = 0;
            for (int j = 1; j <= N - 1; j++)
            {
                e += x[i, j];
            }
            e = e - 1;
            e2 += e * e;
        }
        int e3 = 0;
        for (int j = 1; j <= N - 1; j++)
        {
            int e = 0;
            for (int i = 1; i <= N - 1; i++)
            {
                e += x[i, j];
            }
            e = e - 1;
            e3 += e * e;
        }
        int e4 = 0;
        for (int i = 1; i <= N - 2; i++)
        {
            int e = 0;
            for (int j = 1; j <= N - i; j++)
            {
                e += x[j, i + j - 1];
            }
            e = e - 1;
            e4 += e * e;
        }
        int e5 = 0;
        for (int i = 2; i <= N - 2; i++)
        {
            int e = 0;
            for (int j = 1; j <= N - i; j++)
            {
                e += x[i + j - 1, j];
            }
            e = e - 1;
            e5 += e * e;
        }
        int e6 = 0;
        for (int i = 1; i <= N - 2; i++)
        {
            int e = 0;
            for (int j = 1; j <= N - i; j++)
            {
                e += x[10 - i - j, j];
            }
            e = e - 1;
            e6 += e * e;
        }
        int e7 = 0;
        for (int i = 2; i <= N - 2; i++)
        {
            int e = 0;
            for (int j = 1; j <= N - i; j++)
            {
                e += x[9 - j, i+j-1];
            }
            e = e - 1;
            e7 += e * e;
        }
        int E = e2 + e3 + e4 + e5 + e6 + e7;
        return E;
    }
    private void CalculateWeight(int[,,,] w)
    {
        int o = 0;
        for (int i = 0; i <= N-1; i++)
        {
            for (int k = 0; k <= N-1; k++)
                x[i, k] = 0;
        }
        int C = E(x);
        for(int i=1;i<=N-1;i++)
        {
            for (int j = 1; j <= N - 1; j++)
            {
                x[i,j] = 1;
                w[0,0,i,j] = -(E(x) - C);
                o += 1;
                w[i, j, 0, 0] = w[0, 0, i, j];
                o += 1;
                x[i, j] = 0;
            }
        }
        for (int i = 1; i <= N - 1; i++)
        {
            for (int j = 1; j <= N - 1; j++)
            {
                for(int n=1;n<=N-1;n++)
                {
                    
                    for (int m=1;m<=N-1;m++)
                    {
                        if(i==n&&j==m)
                        {
                            w[i, j, n, m] = 0;
                        }
                        else
                        {
                        x[i,j] = 1;
                        x[n,m] = 1;
                        w[i,j,n,m]=-E(x)-w[0,0,i,j]-w[0,0,n,m]+C;
                        o += 1;
                        x[i, j] = 0;
                        x[n, m] = 0;
                        }
                    }
                }  
            }
        }
    }
    double sum = 0;
    void forrer(int k, int trials, double alpha)//Recursion to calculate A of Boltzmann's Theorial Number
    {//k represents loop in layer k
        int[,] o = new int[N,N];
        for (int i = 0; i <= 1; i++)  //Number of variable loops per layer
        {
            arr[k] = i;
            if (k == (N-1)*(N-1))//Number of layers
            {
                for (int j = 1; j <= N-1; j++)
                {
                    for (int u = 1; u <= N - 1; u++)
                    { 
                            o[j,u] = arr[(j - 1) * (N - 1) + u];                     
                    }
                }
                sum += Math.Exp(-alpha * E(o));
            }

            else forrer(k + 1, trials, alpha);
        }
    }

    private double BoltzmannNumber(double A,int energy,double alpha)
    {
        double B = A * Math.Exp(-alpha * energy);
        return B;
    }
    void Resetarray(int[,]x)
    {
        for (int o = 0; o <= N - 1; o++)
        {
            for (int k = 0; k <= N - 1; k++)
            {
                x[o, k] = 0;
            }
        }
        x[0, 0] = 1;
    }

}
