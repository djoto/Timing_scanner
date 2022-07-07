using Microsoft.VisualBasic.FileIO;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.DepthBasics.TimingScanner
{
    static class TestClass
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;

        private const int BitmapHeight = 400;
        private const int BitmapWidth = 512;

        //private static byte[] depthPixels = null;
        private static byte[,] scaledImage2D = null;
        private static Mat scaledImageMat = null;
        private static Mat rotatedImageMat = null;
        private static Mat sResizedImageMat = null;

        public static byte[,] getScaledImg2D()
        {
            return scaledImage2D;
        }
        public static Mat getScaledImageMat()
        {
            return scaledImageMat;
        }
        public static Mat getRotatedImageMat()
        {
            return rotatedImageMat;
        }
        public static Mat getSResizedImageMat()
        {
            return sResizedImageMat;
        }

        public static void showSubrtactImage(Mat scaled, Mat rotated)
        {
            Mat subtr = scaled - rotated;
            Cv2.ImShow("Diff Image", subtr);
        }

        public static ushort[] readDepthFrameDataFromCSV(string path)
        {
            //TextFieldParser parser = new TextFieldParser(@"C:\Users\Djordje\Documents\TIMING_skener\dordje_nivelisan_pravilan_luk\dordje.csv");
            TextFieldParser parser = new TextFieldParser(path);

            Console.WriteLine(parser.GetType().Name);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            //int total = 0;
            int cnt = 0;
            ushort[] depthFrmData = null;
            //int[] sumArray = null;
            while (!parser.EndOfData)
            {
                //Processing row
                string[] fields = parser.ReadFields();
                depthFrmData = Array.ConvertAll(fields, s => ushort.Parse(s));

                //int[] depthFrmInt = new int[depthFrmData.Length];
                //for (int i = 0; i < depthFrmData.Length; i++)
                //{
                //    depthFrmInt[i] = (int)depthFrmData[i];
                //}
                //int[] depthFrmInt = Array.ConvertAll(depthFrmData, s => (int)s);

                //if (cnt == 0)
                //{
                //    sumArray = depthFrmInt;
                //}
                //sumArray.Zip(depthFrmInt, (x, y)=>x + y);
                //ukupno += ints.Length;
                cnt++;
                //Console.WriteLine(String.Join(",", ints));




                //read only first frame:  !!!
                if (cnt == 1)
                {
                    //for(int i = 0; i < depthFrmInt.Length; i++) {
                    //    depthFrmData[i] = (ushort)((ushort)sumArray[i] / cnt);
                    //}
                    break;
                }
                //Console.WriteLine(fields.Length);
                /*foreach (string field in fields)
                {
                    cnt++;
                    Console.WriteLine(field);
                    Console.WriteLine(cnt);
                }*/
            }

            return depthFrmData;
        }

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
                //this.depthPixels[i] = (byte)(depth >= 0 && depth <= ushort.MaxValue ? (depth / MapDepthToByte) : 0);
                depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
            //this.depthPixels = depthPixels;
        }


        public static Mat Byte1DToMat(int h, int w, byte[] byte1D)
        {
            Mat imageMat = new Mat(h, w, MatType.CV_8UC1);
            imageMat.SetArray(0, 0, byte1D);
            return imageMat;
        }

        public static Mat Byte2DToMat(byte[,] byte2D)
        {
            Mat imageMat = new Mat(byte2D.GetLength(0), byte2D.GetLength(1), MatType.CV_8UC1);
            imageMat.SetArray(0, 0, byte2D);
            return imageMat;
        }

        public static byte[] MatToByte1D(Mat mat)
        {
            byte[] b = new byte[mat.Channels() * mat.Cols * mat.Rows];
            mat.GetArray(0, 0, b);
            return b;
        }

        public static byte[,] MatToByte2D(Mat mat)
        {
            byte[,] b = new byte[mat.Cols, mat.Rows];
            mat.GetArray(0, 0, b);
            return b;
        }



        public static Mat ErodeDilateImage(int h, int w, byte[] depthPixels)
        {
            Mat srcImg = Byte1DToMat(h, w, depthPixels);
            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);

            Mat elementDilate1 = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1));
            Mat elementErode = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(19, 19), new Point(-1, -1));
            //Mat elementErode2 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));
            //Mat elementDilate2 = Cv2.GetStructuringElement(MorphShapes.Cross, new Size(7, 7), new Point(-1, -1));

            Cv2.Dilate(srcImg, dstImg, elementDilate1);
            Cv2.Erode(dstImg, dstImg, elementErode);
            //Cv2.Erode(dstImg, dstImg, elementErode2);
            //Cv2.Dilate(dstImg, dstImg, elementDilate2);

            //Cv2.ImShow("Source Image", srcImg);
            //Cv2.ImShow("Destination Image", dstImg);
            //Cv2.WaitKey(0);

            //dstImg =  this.SobelResultImg(dstImg);
            //dstImg = this.ByteArrayToMat(this.BlackWhiteByteArray(this.MatToByteArray(dstImg))); // prebacivanje u crno bijelu sliku: 0 ili 255
            //dstImg = this.SobelResultImg(dstImg);

            //Cv2.ImShow("Source Image", srcImg);
            //Cv2.ImShow("Destination Image", dstImg);

            //showScaledImage(dstImg);

            return dstImg;

        }

        public static Mat CannyEdgeDetection(Mat matImg)
        {
            Mat src = matImg;
            Mat dst = src;
            Cv2.Canny(src, dst, 0, 255, 3, false);


            Point[][] contours = null;
            HierarchyIndex[] hierarchy = null;
            Mat dstCont = dst;
            Cv2.FindContours(dstCont, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);

            Cv2.DrawContours(dstCont, contours, -1, new Scalar(255, 50, 50), 1);

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

            Cv2.Rectangle(dstCont, maxRect, new Scalar(100, 255, 100), 1);
            //Cv2.Rectangle(dstCont, new Point(rct.X, rct.Y), new Point(rct.X + rct.Width, rct.Y + rct.Height), new Scalar(255, 255, 255), 2);
            Cv2.ImShow("First contour with bounding box", dstCont);


            //Point2f[] corners = Cv2.GoodFeaturesToTrack(dstCont, 2, 0.5, 20, null, 2, false, 0);

            //foreach(Point2f corner in corners)
            //{
            //     float x = corner.X;
            //    float y = corner.Y;
            //    Cv2.Circle(dstCont, (int)x, (int)y, 5, 255, -1);
            //}




            //Cv2.ApproxPolyDP(dst, dstCont,  1, true);

            //Cv2.ImShow("cont", dstCont);

            return dst;
        }

        public static Mat SobelResultImg(int h, int w, Mat depthPixelsMat)
        {
            Mat srcImg = depthPixelsMat;
            Mat dstImg1 = new Mat();
            //Mat dstImg2 = new Mat();
            srcImg.CopyTo(dstImg1);
            //srcImg.CopyTo(dstImg2);

            Cv2.Sobel(srcImg, dstImg1, MatType.CV_8UC1, 1, 0, 3);
            //Cv2.Sobel(srcImg, dstImg2,/MatType.CV_8UC1, 0, 1, 3);


            //byte[] dstImg1Byte = this.MatToByteArray(dstImg1);
            //byte[] dstImg2Byte = this.MatToByteArray(dstImg2);

            //byte[] dstImgByte = dstImg1Byte;
            //dstImgByte.Zip(dstImg2Byte, (x, y) => ((x + y - 1) / 255)*255);
            // Mat dstImg = ByteArrayToMat(h, w, dstImgByte);

            Cv2.ImShow("Source Image sobel", srcImg);
            Cv2.ImShow("Destination Image sobel", dstImg1);

            return dstImg1;
        }

        public static byte[] BlackWhiteByteArray(byte[] depthPixels)
        {
            byte[] newByteImg = depthPixels;
            for (int i = 0; i < depthPixels.Length; i++)
            {
                if (depthPixels[i] != 0)
                {
                    newByteImg[i] = 255;
                }
            }
            return newByteImg;
        }


        public static byte[,] toByte2D(int h, int w, byte[] depthPixels)
        {
            int r = h;
            int c = w;

            byte[,] arr2D = new byte[r, c];
            /*for (int i = 0; i < r * c; i++)
            {
                arr2D[i / r, i % c] = depthPixels[i];
            }*/
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    arr2D[i, j] = depthPixels[i * c + j];
                }
            }
            return arr2D;
        }
        public static byte[] toByte1D(byte[,] array2D)
        {
            int r = array2D.GetLength(0);
            int c = array2D.GetLength(1);

            byte[] arr1D = new byte[r * c];
            //for (int i = 0; i < r * c; i++)
            //arr1D[i] = array2D[i / r, i % c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    arr1D[i * c + j] = array2D[i, j];
                }
            }
            return arr1D;
        }

        public static byte[] hackedCropByteImg(int h, int w, byte[] depthPixels)
        {
            byte[] newByteArr = toByte1D(toByte2D(h, w, depthPixels));
            Cv2.ImShow("Crop image", Byte1DToMat(h, w, newByteArr));
            return newByteArr;
        }

        public static void showRotatedImage(Mat matImg, float angle)
        {
            Mat dst = MatRotate(matImg, angle);
            Cv2.ImShow("Rotated Image", dst);
        }

        public static void showScaledImage(byte[] depthPixels)
        {
            //byte[] depthPixels = MatToByteArray(matImg);
            byte[,] pixels2D = toByte2D(BitmapHeight, BitmapWidth, depthPixels);
            byte[,] scaledPixels2D = scaleBendImage(pixels2D);
            Mat scaledMatImg = Byte1DToMat(scaledPixels2D.GetLength(0), scaledPixels2D.GetLength(1), toByte1D(scaledPixels2D));
            Cv2.ImShow("Scaled Image", scaledMatImg);

            //resize:
            //scaledMatImg = scaledMatImg.Resize(new Size(500, 500));


            scaledImageMat = scaledMatImg;
        }

        public static byte[,] scaleBendImage(byte[,] pixels2D)
        {
            int fromTop = 0;
            int fromLeft = 0;
            int fromRight = 0;
            int fromBottom = 0;

            bool top = false;
            bool left = false;
            bool right = false;
            bool bottom = false;

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

            scaledImage2D = scaledPixels2D;

            return scaledPixels2D;
        }

        public static int[] getArrayY(byte[,] scaledImg)
        {
            int[] array = new int[scaledImg.GetLength(1)];

            for (int j = 0; j < scaledImg.GetLength(1); j++)
            {
                for (int i = (scaledImg.GetLength(0) - 1); i >= 0; i--)
                {
                    if (scaledImg[i, j] != 0)
                    {
                        array[j] = scaledImg.GetLength(0) - i;
                        break;
                    }
                }
            }

            return array;
        }

        public static int getDifferenceBetweenTwoBendImages(Mat m1, Mat m2)
        {
            Mat mOrig = m1;
            Mat mNew = m2;
            mOrig = scaledResize(mOrig, 200);
            mNew = scaledResize(mNew, 200);

            byte[,] byteArrOrig2D = toByte2D(mOrig.Size(0), mOrig.Size(1), MatToByte1D(mOrig));
            byte[,] byteArrNew2D = toByte2D(mNew.Size(0), mNew.Size(1), MatToByte1D(mNew));

            int[] arrOrigY = getArrayY(byteArrOrig2D);
            int[] arrNewY = getArrayY(byteArrNew2D);

            //int[] diffArr = arrOrigY;
            int[] diffArr = new int[arrOrigY.Length];

            for (int i = 0; i < arrOrigY.Length; i++)
            {
                diffArr[i] = Math.Abs(arrOrigY[i] - arrNewY[i]);
            }
            //diffArr.Zip(arrNewY, (x, y) => Math.Abs(x - y));

            //int s = 0;
            //for (int i = 0; i < diffArr.Length; i++)
            //{
            //   s += diffArr[i];
            //}
            return diffArr.Sum();

            //return s;
        }


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

            rotatedImageMat = dst;

            return dst;
        }

        public static Mat scaledResize(Mat matImg, int sizeX)
        {
            float scaledY = (1 - (float)matImg.Size(1) / matImg.Size(0)) * sizeX + sizeX;
            int sizeY = Convert.ToInt32(scaledY) % 2 == 0 ? Convert.ToInt32(scaledY) : Convert.ToInt32(scaledY) + 1;
            sResizedImageMat = matImg.Resize(new Size(sizeY, sizeX));
            return matImg.Resize(new Size(sizeX, sizeY));
        }

        public static void showMatImg(Mat img, string str)
        {
            Cv2.ImShow(str, img);
        }

        public static Mat extractBendFromMat(Mat matImg)
        {
            byte[,] byte2DImg = new byte[matImg.Size(0), matImg.Size(1)];
            byte[] byte1DImg = MatToByte1D(matImg);
            byte2DImg = toByte2D(matImg.Size(0), matImg.Size(1), byte1DImg);
            byte2DImg = scaleBendImage(byte2DImg);

            return Byte1DToMat(byte2DImg.GetLength(0), byte2DImg.GetLength(1), toByte1D(byte2DImg));
        }

        public static Mat setEvenSize(Mat matImg)
        {
            int sizeH = matImg.Size(0);
            int sizeW = matImg.Size(1);

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
                return matImg.Resize(new Size(sizeW, sizeH));
            }
            else
            {
                return matImg;
            }

        }

        public static Mat UpperHalf(Mat matImg)
        {
            return matImg.RowRange(0, matImg.Size(0) / 2);
        }
        public static Mat LowerHalf(Mat matImg)
        {
            return matImg.RowRange(matImg.Size(0) / 2, matImg.Size(0));
        }
        public static Mat RightHalf(Mat matImg)
        {
            return matImg.ColRange(0, matImg.Size(1) / 2);
        }
        public static Mat LeftHalf(Mat matImg)
        {
            return matImg.ColRange(matImg.Size(1) / 2, matImg.Size(1));
        }

        public static Mat flipVertical(Mat imgMat)
        {
            return imgMat.Flip(FlipMode.Y);
        }
        public static Mat flipHorizontal(Mat imgMat)
        {
            return imgMat.Flip(FlipMode.X);
        }

        public static Mat bendSkewCorrectedImg(Mat bendImg)
        {
            int[] arrayHorizontal = new int[91];
            int[] arrayVertical = new int[91];

            Mat upperHalf = null;
            Mat lowerHalf = null;
            Mat leftHalf = null;
            Mat rightHalf = null;

            Mat bendImage = extractBendFromMat(bendImg);

            for (int i = 0; i <= 90; i++)
            {
                //bendImage = extractBendFromMat(bendImage);
                bendImage = MatRotate(bendImg, i);
                bendImage = extractBendFromMat(bendImage);
                bendImage = setEvenSize(bendImage);

                upperHalf = UpperHalf(bendImage);
                lowerHalf = LowerHalf(bendImage);
                leftHalf = LeftHalf(bendImage);
                rightHalf = RightHalf(bendImage);

                Mat diffUpLow = upperHalf - flipHorizontal(lowerHalf);
                Mat diffLeftRight = leftHalf - flipVertical(rightHalf);

                //if (i == 80)
                //{
                //    Cv2.ImShow("diffUpLow" + i.ToString(), upperHalf);
                //    Cv2.ImShow("diffLeftRight" + i.ToString(), lowerHalf);
                //}

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
                return correctHorizontalBend(extractBendFromMat(MatRotate(bendImg, angleH)));
            }
            else
            {
                return correctVerticalBend(extractBendFromMat(MatRotate(bendImg, angleV)));
            }

            //return bendImage;

        }

        /*byte[] mba = null;
        public static string getMbaStr()
        {
            string s = "";
            for (int i = 0; i < this.mba.Length; i++)
            {
                s += this.mba[i].ToString() + ", ";
            }
            return s;

        }*/

        public static Mat correctHorizontalBend(Mat imgMat)
        {
            //srednji red u imgMat
            byte[] middleByteArray = MatToByte1D(imgMat.RowRange(imgMat.Size(0) / 2 - 1, imgMat.Size(0) / 2));
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
                return extractBendFromMat(MatRotate(imgMat, 270)); // lijevo orijentisan, rotiraj za 270 stepeni suprotno kazaljci na satu
            }
            else
            {
                return extractBendFromMat(MatRotate(imgMat, 90));  // desno orijentisan, rotiraj za 90 stepeni suprotno kazaljci na satu
            }

        }

        public static Mat correctVerticalBend(Mat imgMat)
        {
            //srednji red u imgMat
            byte[] middleByteArray = MatToByte1D(imgMat.ColRange(imgMat.Size(1) / 2 - 1, imgMat.Size(1) / 2));

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
                return imgMat; // orijentisan ka gore, znaci ispravno orijentisan, ne rotira se
            }
            else
            {
                return extractBendFromMat(MatRotate(imgMat, 180));  // orijentisan ka dole, rotiraj za 180 stepeni
            }

        }

        public static Mat df(Mat m1, Mat m2)
        {
            return m1 - m2;
        }

        /*public static float[] GetDerivativeArray(int[] arrY, int step, int epsilon)
        {
            int dArrSize = (arrY.Length - 1) / step + 1;
            float[] dArr = new float[dArrSize + 1];
            int cnt = 0;
            for (int i = 0; i < arrY.Length - (arrY.Length - 1) % step; i += step)
            {
                if(i < arrY.Length / 2)
                {
                    if((i - epsilon/2) < 0)
                    {
                        dArr[cnt] = (float)(arrY[i + epsilon / 2] - arrY[i]) / (epsilon / 2);
                    }
                    else
                    {
                        dArr[cnt] = (float)(arrY[i + epsilon / 2] - arrY[i - epsilon / 2]) / epsilon;
                    }
                }
                else
                {
                    if ((i + epsilon / 2) >= arrY.Length)
                    {
                        dArr[cnt] = (float)(arrY[i] - arrY[i - epsilon / 2]) / (epsilon / 2);
                    }
                    else
                    {
                        dArr[cnt] = (float)(arrY[i + epsilon / 2] - arrY[i - epsilon / 2]) / epsilon;
                    }
                }
                cnt++;
            }
            dArr[cnt] = (float)(arrY[arrY.Length - 1] - arrY[arrY.Length - 1 - epsilon / 2]) / (epsilon / 2);
            return dArr;

        }

        public static Mat TestDilateImage(Mat srcImg)
        {
            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);

            Mat elementDilate1 = Cv2.GetStructuringElement(MorphShapes.Ellipse, new Size(3, 3), new Point(-1, -1));

            Cv2.Dilate(srcImg, dstImg, elementDilate1);

            return dstImg;
        }

        public static Mat TestFindCornerPoints(Mat srcImg)
        {
            Mat dstImg = new Mat();
            srcImg.CopyTo(dstImg);
            Point2f[] corners = Cv2.GoodFeaturesToTrack(dstImg, 4, 0.5, 20, null, 2, false, 0);

            foreach(Point2f corner in corners)
            {
                float x = corner.X;
                float y = corner.Y;
                Cv2.Circle(dstImg, (int)x, (int)y, 4, 255, -1);
            }
            //Cv2.ApproxPolyDP(dstImg, dstImg,  1, true);

            //Cv2.ImShow("cont", dstCont);
            return dstImg;
        }*/
    }
}
