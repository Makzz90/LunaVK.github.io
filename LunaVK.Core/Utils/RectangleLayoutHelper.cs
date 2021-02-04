using System;
using System.Collections.Generic;
using Windows.Foundation;
using System.Linq;

namespace LunaVK.Core.Utils
{
    //ThumbnailLayoutManager
    public class RectangleLayoutHelper
    {
        public static List<Rect> CreateLayout(double parent_width, double parent_height, List<Size> childrenRects, double marginBetween)
        {
            List<ThumbAttachment> thumbAttachments = RectangleLayoutHelper.ConvertSizesToThumbAttachments(childrenRects);
            RectangleLayoutHelper.ProcessThumbnails(parent_width, parent_height, thumbAttachments, marginBetween);
            return RectangleLayoutHelper.ConvertProcessedThumbsToRects(thumbAttachments, marginBetween, parent_width);
        }

        private static List<Rect> ConvertProcessedThumbsToRects(List<ThumbAttachment> thumbs, double marginBetween, double width)
        {
            List<Rect> rectList = new List<Rect>(thumbs.Count);
            double num1 = 0.0;
            double widthOfRow = RectangleLayoutHelper.CalculateWidthOfRow(thumbs, marginBetween);
            double num2 = width / 2.0 - widthOfRow / 2.0;
            double num3 = num2;
            for (int index = 0; index < thumbs.Count; ++index)
            {
                ThumbAttachment thumb = thumbs[index];
                rectList.Add(new Rect(num3, num1, thumb.CalcWidth, thumb.CalcHeight));
                if (!thumb.LastColumn && !thumb.LastRow)
                    num3 += thumb.CalcWidth + marginBetween;
                else if (thumb.LastRow)
                    num1 += thumb.CalcHeight + marginBetween;
                else if (thumb.LastColumn)
                {
                    num3 = num2;
                    num1 += thumb.CalcHeight + marginBetween;
                }
            }
            return rectList;
        }

        private static double CalculateWidthOfRow(List<ThumbAttachment> thumbs, double marginBetween)
        {
            double num = 0.0;
            foreach (ThumbAttachment thumb in thumbs)
            {
                num += thumb.CalcWidth;
                num += marginBetween;
                if (!thumb.LastRow)
                {
                    if (thumb.LastColumn)
                        break;
                }
                else
                    break;
            }
            if (num > 0.0)
                num -= marginBetween;
            return num;
        }

        private static double calculateMultiThumbsHeight(List<double> ratios, double width, double margin)
        {
            return (width - (double)(ratios.Count - 1) * margin) / Enumerable.Sum(ratios);
        }

        private static void ProcessThumbnails(double max_width, double max_height, List<ThumbAttachment> thumbs, double marginBetween)
        {
            string str = "";
            int[] numArray = new int[3];
            List<double> doubleList1 = new List<double>();
            int count = thumbs.Count;
            bool error = false;
            foreach (ThumbAttachment thumb in thumbs)
            {
                double ratio = thumb.getRatio();
                if (ratio == -1.0)
                    error = true;
                char orient = ratio > 1.2 ? 'w' : (ratio < 0.8 ? 'n' : 'q');
                str += orient.ToString();
                numArray[RectangleLayoutHelper.oi(orient)]++;
                doubleList1.Add(ratio);
            }
            if (error)
            {
                int i = 0;
            }
            else
            {
                double num1 = doubleList1.Count > 0 ? Enumerable.Sum(doubleList1) / (double)doubleList1.Count : 1.0;
                double margin = marginBetween;
                double w;
                double h;

                if (max_width > 0.0)
                {
                    w = max_width;
                    h = max_height;
                }
                else
                {
                    w = 320.0;
                    h = 210.0;
                }

                double num4 = w / h;
                if (count == 1)
                {
                    if (doubleList1[0] > 0.8)
                        thumbs[0].SetViewSize(w, w / doubleList1[0], false, false);
                    else
                        thumbs[0].SetViewSize(h * doubleList1[0], h, false, false);
                }
                else if (count == 2)
                {
                    if (str == "ww" && num1 > 1.4 * num4 && doubleList1[1] - doubleList1[0] < 0.2)
                    {
                        double height = Math.Min(w / doubleList1[0], Math.Min(w / doubleList1[1], (h - marginBetween) / 2.0));
                        thumbs[0].SetViewSize(w, height, true, false);
                        thumbs[1].SetViewSize(w, height, false, false);
                    }
                    else if (str == "ww" || str == "qq")
                    {
                        double width2 = (w - margin) / 2.0;
                        double height = Math.Min(width2 / doubleList1[0], Math.Min(width2 / doubleList1[1], h));
                        thumbs[0].SetViewSize(width2, height, false, false);
                        thumbs[1].SetViewSize(width2, height, false, false);
                    }
                    else
                    {
                        double width2 = (w - margin) / doubleList1[1] / (1.0 / doubleList1[0] + 1.0 / doubleList1[1]);
                        double width3 = w - width2 - margin;
                        double height = Math.Min(h, Math.Min(width2 / doubleList1[0], width3 / doubleList1[1]));
                        thumbs[0].SetViewSize(width2, height, false, false);
                        thumbs[1].SetViewSize(width3, height, false, false);
                    }
                }
                else if (count == 3)
                {
                    if (str == "www")
                    {
                        double height1 = Math.Min(w / doubleList1[0], (h - marginBetween) * 0.66);
                        thumbs[0].SetViewSize(w, height1, true, false);
                        double width3 = (w - margin) / 2.0;
                        double height2 = Math.Min(h - height1 - marginBetween, Math.Min(width3 / doubleList1[1], width3 / doubleList1[2]));
                        thumbs[1].SetViewSize(width3, height2, false, false);
                        thumbs[2].SetViewSize(width3, height2, false, false);
                    }
                    else
                    {
                        double height1 = h;
                        double width2 = Math.Min(height1 * doubleList1[0], (w - margin) * 0.75);
                        thumbs[0].SetViewSize(width2, height1, false, false);
                        double height2 = doubleList1[1] * (h - marginBetween) / (doubleList1[2] + doubleList1[1]);
                        double height3 = h - height2 - marginBetween;
                        double width3 = Math.Min(w - width2 - margin, Math.Min(height2 * doubleList1[2], height3 * doubleList1[1]));
                        thumbs[1].SetViewSize(width3, height3, false, true);
                        thumbs[2].SetViewSize(width3, height2, false, true);
                    }
                }
                else if (count == 4)
                {
                    if (str == "wwww")
                    {
                        double width2 = w;
                        double height1 = Math.Min(width2 / doubleList1[0], (h - marginBetween) * 0.66);
                        thumbs[0].SetViewSize(width2, height1, true, false);
                        double val2 = (w - 2.0 * margin) / (doubleList1[1] + doubleList1[2] + doubleList1[3]);
                        double width3 = val2 * doubleList1[1];
                        double width4 = val2 * doubleList1[2];
                        double width5 = val2 * doubleList1[3];
                        double height2 = Math.Min(h - height1 - marginBetween, val2);
                        thumbs[1].SetViewSize(width3, height2, false, false);
                        thumbs[2].SetViewSize(width4, height2, false, false);
                        thumbs[3].SetViewSize(width5, height2, false, false);
                    }
                    else
                    {
                        double height1 = h;
                        double width2 = Math.Min(height1 * doubleList1[0], (w - margin) * 0.66);
                        thumbs[0].SetViewSize(width2, height1, false, false);
                        double val2 = (h - 2.0 * marginBetween) / (1.0 / doubleList1[1] + 1.0 / doubleList1[2] + 1.0 / doubleList1[3]);
                        double height2 = val2 / doubleList1[1];
                        double height3 = val2 / doubleList1[2];
                        double height4 = val2 / doubleList1[3];
                        double width3 = Math.Min(w - width2 - margin, val2);
                        thumbs[1].SetViewSize(width3, height2, false, true);
                        thumbs[2].SetViewSize(width3, height3, false, true);
                        thumbs[3].SetViewSize(width3, height4, false, true);
                    }
                }
                else
                {
                    List<double> doubleList2 = new List<double>();
                    if (num1 > 1.1)
                    {
                        foreach (double val2 in doubleList1)
                            doubleList2.Add(Math.Max(1.0, val2));
                    }
                    else
                    {
                        foreach (double val2 in doubleList1)
                            doubleList2.Add(Math.Min(1.0, val2));
                    }
                    Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
                    int num5;
                    dictionary[string.Concat((num5 = count))] = new List<double>()
                    {
                        RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2, w, margin)
                    };
                    for (int index = 1; index <= count - 1; ++index)
                    {
                        int num6;
                        dictionary[index.ToString() + "," + (num6 = count - index)] = new List<double>()
                        {
                          RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2.Sublist<double>(0, index), w, margin),
                          RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index, doubleList2.Count), w, margin)
                        };
                    }
                    for (int index1 = 1; index1 <= count - 2; ++index1)
                    {
                        for (int index2 = 1; index2 <= count - index1 - 1; ++index2)
                        {
                            int num6;
                            dictionary[index1.ToString() + "," + index2 + "," + (num6 = count - index1 - index2)] = new List<double>()
                            {
                                RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2.Sublist<double>(0, index1), w, margin),
                                RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index1, index1 + index2), w, margin),
                                RectangleLayoutHelper.calculateMultiThumbsHeight(doubleList2.Sublist<double>(index1 + index2, doubleList2.Count), w, margin)
                            };
                        }
                    }
                    string index3 = null;
                    double num7 = 0.0;
                    foreach (string key in dictionary.Keys)
                    {
                        List<double> doubleList3 = dictionary[key];
                        double num6 = marginBetween * (double)(doubleList3.Count - 1);
                        foreach (double num8 in doubleList3)
                            num6 += num8;
                        double num9 = Math.Abs(num6 - h);
                        if (key.IndexOf(",") != -1)
                        {
                            string[] strArray = key.Split(',');
                            if (int.Parse(strArray[0]) > int.Parse(strArray[1]) || strArray.Length > 2 && int.Parse(strArray[1]) > int.Parse(strArray[2]))
                                num9 *= 1.1;
                        }
                        if (index3 == null || num9 < num7)
                        {
                            index3 = key;
                            num7 = num9;
                        }
                    }
                    List<ThumbAttachment> thumbAttachmentList1 = new List<ThumbAttachment>((IEnumerable<ThumbAttachment>)thumbs);
                    List<double> doubleList4 = new List<double>(doubleList2);
                    string[] strArray1 = index3.Split(',');
                    List<double> doubleList5 = dictionary[index3];
                    int length = strArray1.Length;
                    int index4 = 0;
                    for (int index1 = 0; index1 < strArray1.Length; ++index1)
                    {
                        int num6 = int.Parse(strArray1[index1]);
                        List<ThumbAttachment> thumbAttachmentList2 = new List<ThumbAttachment>();
                        for (int index2 = 0; index2 < num6; ++index2)
                        {
                            thumbAttachmentList2.Add(thumbAttachmentList1[0]);
                            thumbAttachmentList1.RemoveAt(0);
                        }
                        double num8 = doubleList5[index4];
                        ++index4;
                        int num9 = thumbAttachmentList2.Count - 1;
                        for (int index2 = 0; index2 < thumbAttachmentList2.Count; ++index2)
                        {
                            ThumbAttachment thumbAttachment = thumbAttachmentList2[index2];
                            double num10 = doubleList4[0];
                            doubleList4.RemoveAt(0);
                            double width2 = num10 * num8;
                            double height = num8;
                            int num11 = index2 == num9 ? 1 : 0;
                            thumbAttachment.SetViewSize(width2, height, num11 != 0, false);
                        }
                    }
                }
            }
        }
        
        private static int oi(char orient)
        {
            if (orient == 'n')
                return 1;
            if (orient == 'q')
                return 2;
            return 0;
        }

        private static List<ThumbAttachment> ConvertSizesToThumbAttachments(List<Size> childrenRects)
        {
            return Enumerable.ToList(Enumerable.Select<Size, ThumbAttachment>(childrenRects, (r => new ThumbAttachment()
            {
                Height = r.Height > 0.0 ? r.Height : 100.0,
                Width = r.Width > 0.0 ? r.Width : 100.0
            })));
        }
    }
}
