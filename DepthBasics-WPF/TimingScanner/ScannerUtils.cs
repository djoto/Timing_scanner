using Microsoft.VisualBasic.FileIO;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace Microsoft.Samples.Kinect.DepthBasics.TimingScanner
{
    //using static NewtonInterpolation;
    using static TimingScanner.BendClassifier;
    //using static TimingScanner.TestClass;
    static class ScannerUtils
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;

        /// <summary>
        /// Prikazuje sliku u originalnim dimenzijama
        /// </summary>
        /// <param name="imageWindowTitle">naslov na prozoru u kome se slika otvara</param>
        /// <param name="mat">matrica piksela tipa Mat</param>
        public static void ShowImage(string imageWindowTitle, Mat img)
        {

            Cv2.ImShow(imageWindowTitle, img);
        }

        /// <summary>
        /// Vraca 1D niz neoznacenih 16-bitnih integera koji predstavljaju dubine piksela u jednom frejmu.
        /// </summary>
        /// <param name="path">putanja do .csv fajla u kome je vise frejmova</param>
        public static ushort[] ReadDepthFrameDataFromCSV(string path)
        {
            TextFieldParser parser = new TextFieldParser(path);

            Console.WriteLine(parser.GetType().Name);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            int cnt = 0;
            ushort[] depthFrmData = null;
            while (!parser.EndOfData)
            {
                //Processing row
                string[] fields = parser.ReadFields();
                depthFrmData = Array.ConvertAll(fields, s => ushort.Parse(s));

                cnt++;

                //read only first frame:  !!!
                if (cnt == 1)
                {
                    break;
                }
            }
            return depthFrmData;
        }

        /// <summary>
        /// Mapira niz 16-bitnih integera (short[]) u niz 8-bitnih vrijednosti (byte[]) 
        /// </summary>
        /// <param name="depthFrameData">niz 16-bitnih int vrijednosti dubina piksela</param>
        /// <param name="depthPixels">niz u kome ce biti 8-bitne vrijednost dubina piksela</param>
        /// <param name="minDepth">minimalna vrijednost dubine piksela koja ce se uzeti u obzir</param>
        /// <param name="maxDepth">maksimalna vrijednost dubine piksela koja ce se uzeti u obzir</param>
        public static unsafe void ProcessDepthFrameDataFromCsv(ushort[] depthFrameData, byte[] depthPixels, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value

            // convert depth to a visual representation
            for (int i = 0; i < depthFrameData.Length; ++i)
            {
                // Get the depth for this pixel
                ushort depth = depthFrameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        /// <summary>
        /// Vraca matricu slike tipa Mat (1 channel!) od 1D niza 8-bitnih integera (byte[])
        /// </summary>
        /// <param name="h">broj redova matrice - visina slike</param>
        /// <param name="w">broj kolona matrice - sirina slike</param>
        /// <param name="byte1D">1D niz 16-bitnih int vrijednosti dubina piksela</param>
        public static Mat Byte1DToMat(int h, int w, byte[] byte1D)
        {
            Mat imageMat = new Mat(h, w, MatType.CV_8UC1);
            imageMat.SetArray(0, 0, byte1D);
            return imageMat;
        }

        /// <summary>
        /// Vraca matricu slike tipa Mat (1 channel!) od 2D niza 8-bitnih integera (byte[,])
        /// </summary>
        /// <param name="byte2D">2D niz 16-bitnih int vrijednosti dubina piksela</param>
        public static Mat Byte2DToMat(byte[,] byte2D)
        {
            Mat imageMat = new Mat(byte2D.GetLength(0), byte2D.GetLength(1), MatType.CV_8UC1);
            imageMat.SetArray(0, 0, byte2D);
            return imageMat;
        }

        /// <summary>
        /// Vraca 1D niz 8-bitnih integera (byte[]) od matrice slike tipa Mat (1 channel!)
        /// </summary>
        /// <param name="mat">matrica piksela tipa Mat (1 channel!)</param>
        public static byte[] MatToByte1D(Mat mat)
        {
            byte[] b = new byte[mat.Cols * mat.Rows];
            mat.GetArray(0, 0, b);
            return b;
        }

        /// <summary>
        /// Vraca 2D niz 8-bitnih integera (byte[,]) od matrice slike tipa Mat (1 channel!)
        /// </summary>
        /// <param name="mat">matrica piksela tipa Mat (1 channel!)</param>
        public static byte[,] MatToByte2D(Mat mat)
        {
            byte[,] b = new byte[mat.Rows, mat.Cols];
            mat.GetArray(0, 0, b);
            return b;
        }

        /// <summary>
        /// Vraca matricu slike tipa Mat (1 channel!) sa otklonjenim sumom
        /// </summary>
        /// <param name="srcImg">slika - matrica piksela tipa Mat (1 channel!)</param>
        public static Mat ErodeDilateImage(Mat srcImg)
        {
            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);

            //Mat elementDilate2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(1, 1));
            //Mat elementErode = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7), new Point(3, 3));
            //Mat elementDilate1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(4, 4), new Point(2, 2));

            Mat elementErode1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(4, 4), new Point(1, 1));
            Mat elementDilate1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(7, 7), new Point(3, 3));
            Mat elementErode2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(1, 1));
            
            //Mat elementDilate1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(1, 1));
            //Mat elementErode = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(19, 19), new Point(-1, -1));
            //Mat elementErode = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(13, 13), new Point(6, 6));

            /*
                Mat elementErode1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(5, 5), new Point(-1, -1));
                Mat elementErode2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
                Mat elementErode3 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
                Mat elementErode4 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
                Mat elementErode5 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(1, 1), new Point(-1, -1));
            */
            //Mat elementErode6 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
            //Mat elementErode7 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
            //Mat elementErode8 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
            //Mat elementErode9 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));

            //Mat elementErode6 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));

            //Mat elementErode2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
            //Mat elementDilate2 = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(3, 3), new Point(-1, -1));
            
            
            //Cv2.Dilate(srcImg, dstImg, elementDilate2);
            //Cv2.Erode(dstImg, dstImg, elementErode);
            //Cv2.Dilate(dstImg, dstImg, elementDilate1);

            Cv2.Erode(dstImg, dstImg, elementErode1);
            Cv2.Dilate(dstImg, dstImg, elementDilate1);
            Cv2.Erode(dstImg, dstImg, elementErode2);


            //Cv2.Dilate(srcImg, dstImg, elementDilate1);
            //Cv2.Erode(dstImg, dstImg, elementErode);

            /*
              Cv2.Erode(dstImg, dstImg, elementErode1);
              Cv2.Erode(dstImg, dstImg, elementErode2);
              Cv2.Erode(dstImg, dstImg, elementErode3);
              Cv2.Erode(dstImg, dstImg, elementErode4);
              Cv2.Erode(dstImg, dstImg, elementErode5);
           */
            //Cv2.Erode(dstImg, dstImg, elementErode6);
            //Cv2.Erode(dstImg, dstImg, elementErode7);
            //Cv2.Erode(dstImg, dstImg, elementErode8);
            //Cv2.Erode(dstImg, dstImg, elementErode9);

            //Cv2.Erode(dstImg, dstImg, elementErode2);
            //Cv2.Dilate(dstImg, dstImg, elementDilate2);

            return dstImg;
        }

        /// <summary>
        /// Sluzi za detektovanje ivice oblika
        /// </summary>
        /// <param name="srcImg">slika - matrica piksela tipa Mat (1 channel!)</param>
        public static Mat CannyEdgeDetection(Mat srcImg)
        {
            Mat dst = new Mat();
            srcImg.CopyTo(dst);
            Cv2.Canny(srcImg, dst, 0, 255, 3, false);

            return dst;
        }

        /// <summary>
        /// Sluzi za izdvajanje najvece konture sa slike (najveca kontura je po pravilu kontura savijenog profila!)
        /// Vraca odsjecenu sliku sa najvecom konturom koja se nalazi u srcImg.
        /// </summary>
        /// <param name="srcImg">slika - matrica piksela tipa Mat (1 channel!)</param>
        public static Mat ExtractLargestContour(Mat srcImg, bool onlyFind)
        {
            Point[][] contours = null;
            HierarchyIndex[] hierarchy = null;
            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);
            Cv2.FindContours(dstImg, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            Cv2.DrawContours(dstImg, contours, -1, new Scalar(255, 50, 50), 1);

            Rect maxRect = new Rect();

            for (int i = 0; i < contours.Length; i++)
            {
                Rect rct = Cv2.BoundingRect(contours[i]);
                if (i == 0)
                {
                    maxRect = rct;
                }

                if ((rct.Height * rct.Width) > (maxRect.Height * maxRect.Width))
                {
                    maxRect = rct;
                }

            }

            //podesavamo pravougaonik da ne preklapa ivice profila
            maxRect.X = maxRect.X - 1;
            maxRect.Y = maxRect.Y - 1;
            maxRect.Height = maxRect.Height + 2;
            maxRect.Width = maxRect.Width + 2;

            if (maxRect.Y == -1 || maxRect.X == -1)
            {
                return null;
            }

            Cv2.Rectangle(dstImg, maxRect, new Scalar(100, 255, 100), 1);
            //Cv2.ImShow("First contour with bounding box", dstImg);

            if (onlyFind)
            {
                return dstImg;
            }

            // odsijecanje slike na dimenzije pravougaonika oko konture 
            dstImg = dstImg.RowRange(maxRect.Y + 1, maxRect.Y + maxRect.Height - 1);
            dstImg = dstImg.ColRange(maxRect.X + 1, maxRect.X + maxRect.Width - 1);

            return dstImg;
        }

        /// <summary>
        /// Sluzi za prebacivanje slike u crno-bijelu tako sto ce sve sto nije bilo crno u srcImg biti bijelo
        /// </summary>
        /// <param name="srcImg">slika - matrica piksela tipa Mat (1 channel!)</param>
        public static Mat ToBlackWhiteImage(Mat srcImg)
        {
            byte[] byteImg = MatToByte1D(srcImg);
            for (int i = 0; i < byteImg.Length; i++)
            {
                if (byteImg[i] != 0)
                {
                    byteImg[i] = 255;
                }
            }
            return Byte1DToMat(srcImg.Rows, srcImg.Cols, byteImg);
        }

        /// <summary>
        /// Sluzi za skaliranje slike tako sto odsijeca crni okvir oko konture, tako da ostaje samo slika cije ivice dodiruju ivice konture.
        /// Koristi se kada na slici imamo samo konturu profila na crnoj pozadini.
        /// Dimenzije slike koju funkcija vraca se mijenjaju u odnosu na srcImg. 
        /// </summary>
        /// <param name="srcImg">slika - matrica piksela tipa Mat (1 channel!)</param>
        public static Mat scaleImageToOnlyContour(Mat srcImg)
        {
            int fromTop = 0;
            int fromLeft = 0;
            int fromRight = 0;
            int fromBottom = 0;

            bool top = false;
            bool left = false;
            bool right = false;
            bool bottom = false;

            byte[,] pixels2D = MatToByte2D(srcImg);

            // za odozgo:
            for (int i = 0; i < pixels2D.GetLength(0); i++)
            {
                for (int j = 0; j < pixels2D.GetLength(1); j++)
                {
                    if (pixels2D[i, j] != 0)
                    {
                        fromTop = i;
                        top = true;
                        break;
                    }
                }
                if (top)
                {
                    break;
                }
            }

            // za lijevo:
            for (int j = 0; j < pixels2D.GetLength(1); j++)
            {
                for (int i = 0; i < pixels2D.GetLength(0); i++)
                {
                    if (pixels2D[i, j] != 0)
                    {
                        fromLeft = j;
                        left = true;
                        break;
                    }
                }
                if (left)
                {
                    break;
                }
            }

            // za odozdo
            for (int i = pixels2D.GetLength(0) - 1; i >= 0; i--)
            {
                for (int j = 0; j < pixels2D.GetLength(1); j++)
                {
                    if (pixels2D[i, j] != 0)
                    {
                        fromBottom = i;
                        bottom = true;
                        break;
                    }
                }
                if (bottom)
                {
                    break;
                }
            }

            // za desno:
            for (int j = pixels2D.GetLength(1) - 1; j >= 0; j--)
            {
                for (int i = 0; i < pixels2D.GetLength(0); i++)
                {
                    if (pixels2D[i, j] != 0)
                    {
                        fromRight = j;
                        right = true;
                        break;
                    }
                }
                if (right)
                {
                    break;
                }
            }

            int scaledHeight = fromBottom - fromTop + 1;
            int scaledWidth = fromRight - fromLeft + 1;

            byte[,] scaledPixels2D = new byte[scaledHeight, scaledWidth];
            for (int i = 0; i < scaledHeight; i++)
            {
                for (int j = 0; j < scaledWidth; j++)
                {
                    scaledPixels2D[i, j] = pixels2D[i + fromTop, j + fromLeft];
                }
            }

            return Byte2DToMat(scaledPixels2D);
        }

        /// <summary>
        /// Vraca niz Y koordinata spoljasnje ivice profila koji je okrenut kracima ka gore 
        /// Duzina izlaznog niza jednaka je sirini slike (uzimaju se koordinate po visini za svaki piksel po X).
        /// </summary>
        /// <param name="scaledImg">skalirana slika do ivica profila i pravilno rotirana kracima ka gore</param>
        public static int[] getArrayY(Mat scaledImg)
        {
            byte[,] byteImg2D = MatToByte2D(scaledImg);
            int[] array = new int[scaledImg.Cols];

            for (int j = 0; j < scaledImg.Cols; j++)
            {
                for (int i = (scaledImg.Rows - 1); i >= 0; i--)
                {
                    if (byteImg2D[i, j] != 0)
                    {
                        array[j] = byteImg2D.GetLength(0) - i;
                        break;
                    }
                }
            }

            return array;
        }

        /// <summary>
        /// Vraca niz X koordinata spoljasnje ivice profila koji je okrenut kracima ka gore 
        /// Duzina izlaznog niza jednaka je visini slike (uzimaju se koordinate po sirini za svaki piksel po Y).
        /// </summary>
        /// <param name="scaledImg">skalirana slika do ivica profila i pravilno rotirana kracima ka gore</param>
        public static int[] getArrayX(Mat scaledImg)
        {
            byte[,] byteImg2D = MatToByte2D(scaledImg);
            int[] array = new int[scaledImg.Rows];

            for (int i = 0; i < scaledImg.Rows; i++)
            {
                for (int j = 0; j < scaledImg.Cols; j++)
                {
                    if (byteImg2D[i, j] != 0)
                    {
                        array[i] = j + 1;
                        break;
                    }
                }
            }

            return array;
        }

        /// <summary>
        /// Vraca sliku sa parnim dimenzijama sirine i visine. Potencijalna razlika u odnosu na ulaznu sliku je h+1,w+1
        /// </summary>
        /// <param name="srcImg">matrica piksela slike tipa Mat</param>
        public static Mat setEvenSize(Mat srcImg)
        {
            int sizeH = srcImg.Size(0);
            int sizeW = srcImg.Size(1);

            bool odd = false;

            if (sizeH % 2 != 0)
            {
                sizeH += 1;
                odd = true;
            }
            if (sizeW % 2 != 0)
            {
                sizeW += 1;
                odd = true;
            }
            if (odd)
            {
                return srcImg.Resize(new Size(sizeW, sizeH));
            }
            else
            {
                return srcImg;
            }
        }

        /// <summary>
        /// Vraca sliku promenjenih dimenzija, s tim da se kao argument proslijedjuje samo sirina.
        /// Visina izlazne slike se automatski podesava (povecava/smanjuje) uzimajuci u obzir odnos dimenzija ulazne slike (dst.h = (src.h*dst.w)/src.w
        /// Odnos dimenzija ulazne i izlazne slike ostaje netaknut! (src.h/src.w = dst.h/dst.w)
        /// Sirina i visina izlazne slike ce biti parni brojevi. 
        /// </summary>
        /// <param name="srcImg">matrica piksela slike tipa Mat</param>
        /// <param name="sizeX">sirina nove slike na izlazu</param>
        public static Mat scaledResize(Mat srcImg, int sizeX)
        {
            float scaledY = (srcImg.Rows * sizeX) / srcImg.Cols;
            //float scaledY = (1 - (float)srcImg.Size(1) / srcImg.Size(0)) * sizeX + sizeX;
            //int sizeY = Convert.ToInt32(scaledY) % 2 == 0 ? Convert.ToInt32(scaledY) : Convert.ToInt32(scaledY) + 1;
            int sizeY = Convert.ToInt32(scaledY);
            Mat retImg = srcImg.Resize(new Size(sizeX, sizeY));
            return setEvenSize(retImg);
        }

        /// <summary>
        /// Vraca sumu apsolutne razlike 2 slike na kojima su izdvojeni lukovi, skalirani do ivica i rotirani kracima ka gore
        /// Sluzi za poredjenje 2 luka. Sto je izlazna suma manja lukovi su slicniji.
        /// </summary>
        /// <param name="img1">skalirana slika do ivica profila i pravilno rotirana kracima ka gore</param>
        /// <param name="img2">skalirana slika do ivica profila i pravilno rotirana kracima ka gore</param>
        public static int getDifferenceBetweenTwoBendImages(Mat img1, Mat img2)
        {
            Mat m1 = scaledResize(img1, 200);
            Mat m2 = scaledResize(img2, 200);

            int[] arr1Y = getArrayY(m1);
            int[] arr2Y = getArrayY(m2);

            int[] diffArr = new int[arr1Y.Length];

            for (int i = 0; i < arr1Y.Length; i++)
            {
                diffArr[i] = Math.Abs(arr1Y[i] - arr2Y[i]);
            }

            return diffArr.Sum();
        }


        /// <summary>
        /// Vraca gornju polovinu slike. Dimenzije izlazne slike su h=srcImg.h/2, w=srcImg.w
        /// </summary>
        /// <param name="srcImg">slika sa parnim vrijednostima visine (h) i sirine (w)</param>
        public static Mat UpperHalf(Mat srcImg)
        {
            return srcImg.RowRange(0, srcImg.Size(0) / 2);
        }

        /// <summary>
        /// Vraca donju polovinu slike. Dimenzije izlazne slike su h=srcImg.h/2, w=srcImg.w
        /// </summary>
        /// <param name="srcImg">slika sa parnim vrijednostima visine (h) i sirine (w)</param>
        public static Mat LowerHalf(Mat srcImg)
        {
            return srcImg.RowRange(srcImg.Size(0) / 2, srcImg.Size(0));
        }

        /// <summary>
        /// Vraca lijevu polovinu slike. Dimenzije izlazne slike su h=srcImg.h, w=srcImg.w/2
        /// </summary>
        /// <param name="srcImg">slika sa parnim vrijednostima visine (h) i sirine (w)</param>
        public static Mat LeftHalf(Mat srcImg)
        {
            return srcImg.ColRange(0, srcImg.Size(1) / 2);
        }

        /// <summary>
        /// Vraca desnu polovinu slike. Dimenzije izlazne slike su h=srcImg.h, w=srcImg.w/2
        /// </summary>
        /// <param name="srcImg">slika sa parnim vrijednostima visine (h) i sirine (w)</param>
        public static Mat RightHalf(Mat srcImg)
        {
            return srcImg.ColRange(srcImg.Size(1) / 2, srcImg.Size(1));
        }

        /// <summary>
        /// Vraca sliku okrenutu u odnosu na vertikalnu (Y) osu. Dimenzije izlazne slike su iste kao i ulazne
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat flipVertical(Mat srcImg)
        {
            return srcImg.Flip(FlipMode.Y);
        }

        /// <summary>
        /// Vraca sliku okrenutu u odnosu na horizontalnu (X) osu. Dimenzije izlazne slike su iste kao i ulazne
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat flipHorizontal(Mat srcImg)
        {
            return srcImg.Flip(FlipMode.X);
        }

        /// <summary>
        /// Rotira sliku za odredjeni ugao suprotno kazaljci na satu. Ne gube se konture sa slike prilikom rotiranja jer se dimenzije slike koju funkcija vraca mijenjaju
        /// </summary>
        /// <param name="src">matrica slike tipa Mat</param>
        /// <param name="angle">ugao za koji rotiramo sliku</param>
        public static Mat MatRotate(Mat src, float angle)
        {
            Mat dst = new Mat();
            Point2f center = new Point2f(src.Cols / 2, src.Rows / 2);
            Mat rot = Cv2.GetRotationMatrix2D(center, angle, 1);
            Size2f s2f = new Size2f(src.Size().Width, src.Size().Height);
            Rect box = new RotatedRect(new Point2f(0, 0), s2f, angle).BoundingRect();
            double xx = rot.At<double>(0, 2) + box.Width / 2 - src.Cols / 2;
            double zz = rot.At<double>(1, 2) + box.Height / 2 - src.Rows / 2;
            rot.Set(0, 2, xx);
            rot.Set(1, 2, zz);
            Cv2.WarpAffine(src, dst, rot, box.Size);

            return ToBlackWhiteImage(dst);
        }

        /// <summary>
        /// Ispravlja sliku na kojoj je samo profil tako da bude okrenut kracima ka gore. Vraca skaliranu sliku do ivica profila, koja nije istih dimenzija kao srcImg
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat correctRotation(Mat srcImg)
        {
            int[] arrayHorizontal = new int[91];
            int[] arrayVertical = new int[91];

            Mat upperHalf = null;
            Mat lowerHalf = null;
            Mat leftHalf = null;
            Mat rightHalf = null;

            Mat bendImage = scaleImageToOnlyContour(srcImg);

            for (int i = 0; i <= 90; i++)
            {
                bendImage = MatRotate(srcImg, i);
                bendImage = scaleImageToOnlyContour(bendImage);
                bendImage = setEvenSize(bendImage);

                upperHalf = UpperHalf(bendImage);
                lowerHalf = LowerHalf(bendImage);
                leftHalf = LeftHalf(bendImage);
                rightHalf = RightHalf(bendImage);

                Mat diffUpLow = upperHalf - flipHorizontal(lowerHalf);
                Mat diffLeftRight = rightHalf - flipVertical(leftHalf);

                arrayHorizontal[i] = (int)diffUpLow.Sum();
                arrayVertical[i] = (int)diffLeftRight.Sum();

            }

            int minV = 0;
            int angleV = 0;
            minV = arrayVertical[0];
            for (int i = 0; i <= 90; i++)
            {
                if (arrayVertical[i] < minV)
                {
                    minV = arrayVertical[i];
                    angleV = i;
                }
            }

            int minH = 0;
            int angleH = 0;
            minH = arrayHorizontal[0];
            for (int i = 0; i <= 90; i++)
            {
                if (arrayHorizontal[i] < minH)
                {
                    minH = arrayHorizontal[i];
                    angleH = i;
                }
            }

            if (minH < minV)
            {
                return correctHorizontalBend(scaleImageToOnlyContour(MatRotate(srcImg, angleH)));
            }
            else
            {
                return correctVerticalBend(scaleImageToOnlyContour(MatRotate(srcImg, angleV)));
            }
        }

        /// <summary>
        /// Ispravlja sliku na kojoj je samo profil pravilno okrenut kracima ka desno ili kracima ka lijevo tako da bude okrenut kracima ka gore. Vraca skaliranu sliku do ivica profila, koja ima transponovane dimanzije u odnosu na srcImg
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat correctHorizontalBend(Mat srcImg)
        {
            //srednji red u imgMat
            byte[] middleByteArray = MatToByte1D(srcImg.RowRange(srcImg.Size(0) / 2 - 1, srcImg.Size(0) / 2));
            //this.mba = middleByteArray;

            bool leftOrientation = false;
            bool rightOrientation = false;

            for (int j = 0; j < middleByteArray.Length; j++)
            {
                if (middleByteArray[j] != 0)
                {
                    rightOrientation = true;
                    break;
                }
                if (middleByteArray[middleByteArray.Length - j - 1] != 0)
                {
                    leftOrientation = true;
                    break;
                }
            }

            if (leftOrientation)
            {
                return scaleImageToOnlyContour(MatRotate(srcImg, 270)); // lijevo orijentisan, rotiraj za 270 stepeni suprotno kazaljci na satu
            }
            else
            {
                return scaleImageToOnlyContour(MatRotate(srcImg, 90));  // desno orijentisan, rotiraj za 90 stepeni suprotno kazaljci na satu
            }

        }

        /// <summary>
        /// Ispravlja sliku na kojoj je samo profil pravilno okrenut kracima ka gore ili kracima ka dole tako da bude sigurno okrenut kracima ka gore. Vraca skaliranu sliku do ivica profila, koja dimanzije kao srcImg
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat correctVerticalBend(Mat srcImg)
        {
            //srednji red u imgMat
            byte[] middleByteArray = MatToByte1D(srcImg.ColRange(srcImg.Size(1) / 2 - 1, srcImg.Size(1) / 2));

            bool upOrientation = false;
            bool downOrientation = false;

            for (int i = 0; i < middleByteArray.Length; i++)
            {
                if (middleByteArray[i] != 0)
                {
                    downOrientation = true;
                    break;
                }
                if (middleByteArray[middleByteArray.Length - i - 1] != 0)
                {
                    upOrientation = true;
                    break;
                }
            }

            if (upOrientation)
            {
                return srcImg; // orijentisan ka gore, znaci ispravno orijentisan, ne rotira se
            }
            else
            {
                return scaleImageToOnlyContour(MatRotate(srcImg, 180));  // orijentisan ka dole, rotiraj za 180 stepeni
            }

        }

        /// <summary>
        /// Vraca sliku novih dimenzija
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat ResizeImage(Mat srcImg, int h, int w)
        {
            return srcImg.Resize(new Size(h, w));
        }

        /// <summary>
        /// Podesava sliku tako da visina bude jednaka sirini. Ako je visina bila veca slika se odsijeca, a ako je visina bila manja po visini se dodaje crni okvir
        /// </summary>
        /// <param name="srcImg">matrica slike tipa Mat</param>
        public static Mat AdaptHeightToWidth(Mat srcImg)
        {

            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);

            int h = srcImg.Rows;
            int w = srcImg.Cols;

            if (h >= w)
            {
                dstImg = dstImg.RowRange(h - w, h);
            }
            else
            {
                Cv2.CopyMakeBorder(dstImg, dstImg, w - h, 0, 0, 0, BorderTypes.Constant, 0);
            }

            return dstImg;

        }

        /// <summary>
        /// Prebacuje niz vrijednosti tipa int u niz vrijednosti tipa float
        /// </summary>
        /// <param name="arr">ulazni niz int[]</param>
        public static float[] IntToFloatArray(int[] arr)
        {
            float[] floatArr = new float[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                floatArr[i] = (float)arr[i];
            }
            return floatArr;
        }

        /// <summary>
        /// Implementira average pooling tehniku za uglacavanje vrijednosti odbiraka
        /// </summary>
        /// <param name="srcArr">ulazni niz float[]</param>
        /// <param name="filterSize">duzina filtera (mora biti neparan broj! - ako nije, funkcija vraca ulazni niz)</param>
        /// <param name="numIter">broj iteracija kroz koji ce se obavljati uglacavanje sa definisanim filterom (default 1)</param>
        public static float[] AveragePoolingSmooth1D(float[] srcArr, int filterSize, int numIter = 1)
        {
            float[] dstArr = new float[srcArr.Length];
            srcArr.CopyTo(dstArr, 0);
            if (filterSize % 2 != 0)
            {
                for (int n = 0; n < numIter; n++)
                {
                    for (int i = 0; i < dstArr.Length; i++)
                    {
                        if (i == 0 || i == (dstArr.Length - 1))
                        {
                            continue;
                        }
                        if (i < filterSize / 2)
                        {
                            int toLeft = i;
                            float sum = 0.0f;
                            for (int k = 1; k <= toLeft; k++)
                            {
                                sum += dstArr[i - k];
                            }
                            sum += dstArr[i];
                            for (int k = 1; k <= filterSize / 2; k++)
                            {
                                sum += dstArr[i + k];
                            }
                            dstArr[i] = sum / (filterSize / 2 + toLeft + 1);
                        }
                        else if (i > (dstArr.Length - filterSize / 2 - 1))
                        {
                            int toRight = dstArr.Length - i - 1;
                            float sum = 0.0f;
                            for (int k = 1; k <= toRight; k++)
                            {
                                sum += dstArr[i + k];
                            }
                            sum += dstArr[i];
                            for (int k = 1; k <= filterSize / 2; k++)
                            {
                                sum += dstArr[i - k];
                            }
                            dstArr[i] = sum / (filterSize / 2 + toRight + 1);
                        }
                        else
                        {
                            float sum = 0.0f;
                            for (int k = 1; k <= filterSize / 2; k++)
                            {
                                sum += dstArr[i - k];
                            }
                            sum += dstArr[i];
                            for (int k = 1; k <= filterSize / 2; k++)
                            {
                                sum += dstArr[i + k];
                            }
                            dstArr[i] = sum / filterSize;
                        }
                    }
                }
                return dstArr;
            }
            else
            {
                return dstArr;
            }
        }

        public static float[] GetSlopeArray(float[] arr)
        {
            float[] arrAngles = new float[arr.Length];
            for(int i = 0; i < arr.Length; i++)
            {
                double angle = 0;
                if( i == 0)
                {
                    angle = Math.Atan(Math.Abs(arr[i + 2] - arr[i]) / 2.0) * 180.0 / Math.PI;
                    arrAngles[i] = (float)angle;
                }
                else if (i == arr.Length - 1)
                {
                    angle = Math.Atan(Math.Abs(arr[i] - arr[i - 2]) / 2.0) * 180.0 / Math.PI;
                    arrAngles[i] = (float)angle;
                }
                else
                {
                    angle = Math.Atan(Math.Abs(arr[i + 1] - arr[i - 1]) / 2.0) * 180.0 / Math.PI;
                    arrAngles[i] = (float)angle;
                }
            }
            return arrAngles;
        }
        


    }
}
