using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.DepthBasics.TimingScanner
{
    using static TimingScanner.ScannerUtils;
    //using static TimingScanner.TestClass;
    static class BendClassifier
    {
        public static string detectBend1FromHalf(Mat srcImg, float err1, float err2)
        {
            string isBend1 = "";
            Mat img = new Mat();
            srcImg.CopyTo(img);

            int scaleWidth = srcImg.Cols * 2;

            int[] arr = getArrayY(img);
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ", ";
            }

            float[] floatArr = IntToFloatArray(arr);
            floatArr = AveragePoolingSmooth1D(floatArr, 5);
            floatArr = AveragePoolingSmooth1D(floatArr, 3);
            string strA = "";
            for (int i = 0; i < floatArr.Length; i++)
            {
                strA += floatArr[i].ToString() + ", ";
            }

            int r = scaleWidth / 2;
            
            float rApprox = floatArr[0];
            float diffR = Math.Abs(rApprox - r);
            if (diffR < err1 * r)
            {
                isBend1 += " true0";  // ako je visina prvog piksela, sa greskom od arr1, jednaka predvidjenom poluprecniku koji bi trebalo da je jednak sirini slike 
            }
            else
            {
                isBend1 += " false0";
            }


            string strCircEq = "";
            int cntBad = 0;
            int maxBadSequence = 0;
            for(int i = 0; i < scaleWidth / 2; i++)
            {
                if ((Math.Pow(floatArr.Length - floatArr[i], 2) + Math.Pow(floatArr.Length - i + 1, 2) - Math.Pow(r, 2)) > (0.05 * Math.Pow(r, 2))) //jednacina kruznice - tolerancija 5% od poluprecnika na kvadrat
                {
                    cntBad += 1;
                }
                else
                {
                    cntBad = 0;
                }
                if (cntBad > maxBadSequence)
                {
                    maxBadSequence = cntBad;
                }
                strCircEq += (Math.Pow(floatArr.Length - floatArr[i], 2) + Math.Pow(floatArr.Length - i + 1, 2) - Math.Pow(r, 2)).ToString() + ", "; 
            }
            if (maxBadSequence <= 3)
            {
                isBend1 += " true1"; // ako svaka tacka luka odgovara tacki kruznice (sa tolerancijom od max 3 vrijednosti za redom koje su za 5% razlicite) predvidjenog poluprecnika polovine sirine slike (lijeve strane luka)
            }
            else
            {
                isBend1 += " false1";
            }


            float bendMiddPoint = r - r * (float)Math.Sqrt(2.0f) / 2.0f;
            float bendMiddPointApprox = (floatArr[(int)Math.Floor(bendMiddPoint) - 1] + floatArr[(int)Math.Ceiling(bendMiddPoint) - 1]) / 2;
            float diffMiddPoint = Math.Abs(bendMiddPoint - bendMiddPointApprox);
            if (diffMiddPoint < err2 * bendMiddPoint)
            {
                isBend1 += " true2";  // udaljenost za vrijednost a *** POTREBNO OBJESNJENJE!
            }
            else
            {
                isBend1 += " false2";
            }

            //double angle = Math.Atan(Math.Abs(floatArr[0] - floatArr[1])) * 180.0 / Math.PI;

            //isBend1 = str + "\n\n" + strA + "\n\n" + arr.Length.ToString() + "\n\n" + floatArr.Length.ToString() + "\n\n" + angle.ToString() + "\n\n" + floatArr[57].ToString() + " " + floatArr[58].ToString();

            //return str + "\n\n" + strA + "\n\n" + strCircEq + "\n\n" + isBend1;
            return "\n\n" + isBend1;
        }
        public static string detectBend1(Mat srcImg, float err1, float err2)
        {
            int scaleWidth = 400;
            Mat img = new Mat();
            srcImg.CopyTo(img);

            img = scaledResize(img, scaleWidth);
            ShowImage("Skalirane dimenzije - sirina 400", img);
            img = LeftHalf(img);
            ShowImage("Lijeva polovina", img);

            return detectBend1FromHalf(img, err1, err2);
              
        }

        public static string detectBend2(Mat srcImg)
        {
            Mat img = new Mat();
            srcImg.CopyTo(img);

            img = scaleImageToOnlyContour(MatRotate(img, 315));
            ShowImage("Rotirana za 315", img);

            return detectBend2Rotated45(img);
        }

        public static string detectBend2Rotated45(Mat img)
        {
            string isBend2 = "";
            //Mat img = new Mat();
            //srcImg.CopyTo(img);

            //img = scaleImageToOnlyContour(MatRotate(img, 315));
            //ShowImage("Rotirana za 315", img);

            int scaleWidth = 400;
            img = scaledResize(img, scaleWidth);
            ShowImage("Skalirane dimenzije - sirina 400", img);

            int[] arr = getArrayY(img);
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ", ";
            }

            float[] floatArr = IntToFloatArray(arr);
            floatArr = AveragePoolingSmooth1D(floatArr, 5);
            floatArr = AveragePoolingSmooth1D(floatArr, 3);
            string strA = "";
            for (int i = 0; i < floatArr.Length; i++)
            {
                strA += floatArr[i].ToString() + ", ";
            }

            int pointToFlat = -1;
            for (int i = 0; i < floatArr.Length; i++)
            {
                if (floatArr[i] < 1.5f && pointToFlat == -1)
                {
                    pointToFlat = i;
                }
            }

            int sequenceGreaterThanTwo = 0;
            int GtTwoTolerantion = (int)Math.Ceiling(0.02f * scaleWidth);
            for (int i = 0; i < GtTwoTolerantion + 1; i++)
            {
                if (floatArr[floatArr.Length - i - 1] >= 2.0f)
                {
                    sequenceGreaterThanTwo += 1;
                }
                else
                {
                    sequenceGreaterThanTwo = 0;
                }
            }
            if (sequenceGreaterThanTwo > GtTwoTolerantion)
            {
                isBend2 += " false1";
            }
            
            if (isBend2 == "")
            {
                isBend2 += " true1";
                //return str + "\n\n" + strA + "\n\n" + " true1";
                int oldWidth = scaleWidth;
                int newWidth = (int)Math.Ceiling((float)(pointToFlat + 1) / 0.9f);
                if(newWidth > oldWidth)
                {
                    newWidth = oldWidth;
                }
                return isBend2 + "\n\n" + detectBend1FromHalf(img.ColRange(0, newWidth), 0.03f, 0.05f);
            }


            return isBend2 + "\n\n" + str + "\n\n" + strA;
        }

        public static string detectBend4(Mat srcImg)
        {
            Mat img = new Mat();
            srcImg.CopyTo(img);

            img = scaleImageToOnlyContour(LeftHalf(img));
            ShowImage("Lijeva polovina za luk 4", img);

            return detectBend2Rotated45(img);
        }


        public static string detectBend6(Mat srcImg)
        {
            string isBend6 = "";

            Mat img = new Mat();
            srcImg.CopyTo(img);

            img = scaleImageToOnlyContour(LeftHalf(img));
            ShowImage("Lijeva polovina za luk 5", img);

            int scaleWidth = 200;
            img = scaledResize(img, scaleWidth);
            ShowImage("Skalirane dimenzije za luk 6 - sirina 200", img);

            int[] arrY = getArrayY(img);
            string strY = "";
            for (int i = 0; i < arrY.Length; i++)
            {
                strY += arrY[i].ToString() + ", ";
            }

            if ((arrY[0] + arrY[1]) / 2 > (scaleWidth + 0.05 * scaleWidth))
            {
                isBend6 = " true1";
            }
            else
            {
                isBend6 = " false1";
            }

            return isBend6;
        }


        public static string isSectionWithoutFlat(Mat srcImg)
        {
            string isSecWithoutFlat = "";

            Mat img = new Mat();
            srcImg.CopyTo(img);

            int scaleWidth = 200;
            img = scaledResize(img, scaleWidth);
            int[] arrYBeforeHalf = getArrayY(img);

            img = scaleImageToOnlyContour(LeftHalf(img));
            ShowImage("Lijeva polovina za isjecak bez ravnog", img);

            int[] arrY = getArrayY(img);
            string strY = "";
            for (int i = 0; i < arrY.Length; i++)
            {
                strY += arrY[i].ToString() + ", ";
            }

            float[] floatArrY = IntToFloatArray(arrY);
            floatArrY = AveragePoolingSmooth1D(floatArrY, 5);
            floatArrY = AveragePoolingSmooth1D(floatArrY, 3);
            string strAy = "";
            for (int i = 0; i < floatArrY.Length; i++)
            {
                strAy += floatArrY[i].ToString() + ", ";
            }

            //recunamo R preko a i b (formula iz sveske)
            float a = (float)arrYBeforeHalf.Length / 2.0f;
            float b = (float)(arrYBeforeHalf[0] + arrYBeforeHalf[arrYBeforeHalf.Length - 1]) / 2.0f;
            float r = (float)(Math.Pow(a, 2) + Math.Pow(b, 2)) / (2.0f * b); // formula iz sveske

            isSecWithoutFlat = isBendSectionArrFromHalf(floatArrY, r, 0.03f, 3);

            return "Poluprecnik: " + Math.Pow(r, 2).ToString() + " " + r.ToString() + " " + a.ToString() + " " + b.ToString() + "\n\n" + isSecWithoutFlat;

        }

        public static string isBendSectionArrFromHalf(float[] samples, float r, float errFromCircle = 0.05f, int allowedBadSequence = 3)
        {
            string isSectionArr = "";

            //string strCircDiff = "";
            int cntBad = 0;
            int maxBadSequence = 0;
            //string provjeraNiza = "";
            for (int i = 0; i < samples.Length; i++)
            {
                if (Math.Abs(Math.Pow(r - samples[i], 2) + Math.Pow(samples.Length - i - 1, 2) - Math.Pow(r, 2)) > (errFromCircle * Math.Pow(r, 2)))
                //if ((Math.Pow(samples.Length - samples[i], 2) + Math.Pow(samples.Length - i + 1, 2) - Math.Pow(r, 2)) > (errFromCircle * Math.Pow(r, 2))) //jednacina kruznice - tolerancija 5% od poluprecnika na kvadrat
                {
                    cntBad += 1;
                }
                else
                {
                    cntBad = 0;
                }
                if (cntBad > maxBadSequence)
                {
                    maxBadSequence = cntBad * 1;
                }
                //strCircDiff += (Math.Pow(samples.Length - samples[i], 2) + Math.Pow(samples.Length - i + 1, 2) - Math.Pow(r, 2)).ToString() + ", ";
                //strCircDiff += Math.Abs(Math.Pow(r - samples[i], 2) + Math.Pow(samples.Length - i + 1, 2) - Math.Pow(r, 2)).ToString() + ", ";
                //provjeraNiza += samples[i].ToString() + ", ";
            }
            if (maxBadSequence <= allowedBadSequence)
            {
                isSectionArr += " true1"; // ako svaka tacka luka odgovara tacki kruznice (sa tolerancijom od max allowedBadSequence vrijednosti za redom koje su najmanje za errFromCircle*(r^2) razlicite) predvidjenog poluprecnika polovine sirine slike (lijeve strane luka)
            }
            else
            {
                isSectionArr += " false1";
            }

            //return isSectionArr + "\n\n" + strCircDiff + "\n\n" + provjeraNiza;
            return isSectionArr;
        }


        public static string detectBend3or5(Mat srcImg)
        {
            string isBend3 = "";
            string isBend5 = "";

            Mat img = new Mat();
            srcImg.CopyTo(img);

            img = scaleImageToOnlyContour(LeftHalf(img));
            ShowImage("Lijeva polovina za luk 5", img);

            int scaleWidth = 200;
            img = scaledResize(img, scaleWidth);
            ShowImage("Skalirane dimenzije - sirina 200 - Luk 5", img);

            int[] arrY = getArrayY(img);
            string strY = "";
            for (int i = 0; i < arrY.Length; i++)
            {
                strY += arrY[i].ToString() + ", ";
            }

            float[] floatArrY = IntToFloatArray(arrY);
            floatArrY = AveragePoolingSmooth1D(floatArrY, 5);
            floatArrY = AveragePoolingSmooth1D(floatArrY, 3);
            string strAy = "";
            for (int i = 0; i < floatArrY.Length; i++)
            {
                strAy += floatArrY[i].ToString() + ", ";
            }

            int x2percent20 = (int)Math.Ceiling(0.2f * arrY.Length);
            int x1 = 0;
            float y1 = floatArrY[x1];
            float y2 = floatArrY[x2percent20];

            // provlacimo pravu kroz (x1, y1) i (x2percent20, y2)
            float n = floatArrY[x1];
            float k = (x2percent20 - x1) / (y1 - y2);

            int cntBad = 0;
            int maxBadSequence = 0;
            float errFromLine = 0.03f;
            int allowedBadSequence = 3;
            for (int i = 0; i < x2percent20; i++)
            {
                if (Math.Abs((k * i + n) - floatArrY[i]) > (errFromLine * floatArrY[i]))
                {
                    cntBad += 1;
                }
                else
                {
                    cntBad = 0;
                }
                if (cntBad > maxBadSequence)
                {
                    maxBadSequence = cntBad * 1;
                }
            }
            if (maxBadSequence <= allowedBadSequence)
            {
                isBend3 += "true"; // nema odstupanja od prave veceg od dozvoljenog, pa se luk klasifikuje kao isjecak sa ravnim produzetkom
            }
            else
            {
                isBend5 += "true";
            }

            if (isBend3 == "true")
            {
                return "Class 3: " + isBend3;
            }
            else
            {
                return "Class 5: " + isBend5;
            }
        }

    }
}
