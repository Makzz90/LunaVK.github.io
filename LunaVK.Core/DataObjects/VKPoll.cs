using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using LunaVK.Core.Json;
using LunaVK.Core.Utils;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using LunaVK.Core.Framework;
using System.IO;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Представляет опрос ВКонтакте.
    /// </summary>
    public class VKPoll : IBinarySerializable
    {
        /// <summary>
        /// Идентификатор опроса.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Идентификатор владельца опроса.
        /// </summary>
        public int owner_id { get; set; }

        /// <summary>
        /// Время создания опроса.
        /// </summary>
        [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
        public DateTime created { get; set; }

        /// <summary>
        /// Текст вопроса.
        /// </summary>
        public string question { get; set; }

        /// <summary>
        /// Общее количество ответивших пользователей.
        /// </summary>
        public int votes { get; set; }

        /// <summary>
        /// Массив, содержащий объекты с вариантами ответа на вопрос в опросе.
        /// </summary>
        public List<VKPollAnswers> answers { get; set; }

        /// <summary>
        /// Является ли опрос анонимным.
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool anonymous { get; set; }

        /// <summary>
        /// допускает ли опрос выбор нескольких вариантов ответа. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool multiple { get; set; }

        /// <summary>
        /// идентификаторы вариантов ответа, выбранных текущим пользователем. 
        /// </summary>
        public List<int> answer_ids { get; set; }

        /// <summary>
        /// дата завершения опроса в Unixtime. 0, если опрос бессрочный. 
        /// </summary>
        public int end_date { get; set; }

        /// <summary>
        /// является ли опрос завершенным. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool closed { get; set; }

        /// <summary>
        /// прикреплён ли опрос к обсуждению. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_board { get; set; }

        /// <summary>
        /// можно ли отредактировать опрос. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_edit { get; set; }

        /// <summary>
        /// можно ли проголосовать в опросе. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_vote { get; set; }

        /// <summary>
        /// можно ли пожаловаться на опрос. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_report { get; set; }

        /// <summary>
        /// можно ли поделиться опросом. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_share { get; set; }

        /// <summary>
        /// идентификатор автора опроса. 
        /// </summary>
        public int author_id { get; set; }

        /// <summary>
        /// фотография — фон сниппета опроса. Объект фотографии. 
        /// </summary>
        public PollPhoto photo { get; set; }

        public PollBacground background { get; set; }

        /// <summary>
        /// идентификаторы 3 друзей, которые проголосовали в опросе. 
        /// </summary>
        public List<PollFriend> friends { get; set; }

        public class PollFriend
        {
            public int id { get; set; }
        }

        public class PollPhoto
        {
            public int id { get; set; }

            public string color { get; set; }

            public List<VKImageWithSize> images { get; set; }
        }

        public class PollBacground
        {
            /// <summary>
            /// идентификатор фона. 
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// тип фона. Возможные значения: gradient, tile.
            /// </summary>
            public string type { get; set; }

            /// <summary>
            /// (для type = gradient) угол градиента по оси X. 
            /// </summary>
            public int angle { get; set; }

            /// <summary>
            /// HEX-код замещающего цвета (без #). 
            /// </summary>
            public string color { get; set; }

            /// <summary>
            /// (для type = tile) ширина плитки паттерна. 
            /// </summary>
            public int width { get; set; }

            /// <summary>
            /// (для type = tile) высота плитки паттерна. 
            /// </summary>
            public int height { get; set; }

            /// <summary>
            /// (для type = tile) изображение плитки паттерна. Массив объектов изображений. 
            /// </summary>
            [JsonConverter(typeof(SizesToDictionaryConverter))]
            public Dictionary<char, VKImageWithSize> images { get; set; }

            /// <summary>
            /// (для type = gradient) точки градиента.
            /// </summary>
            public List<BacgroundPoint> points { get; set; }

            public class BacgroundPoint
            {
                /// <summary>
                /// положение точки
                /// </summary>
                public float position { get; set; }

                /// <summary>
                /// HEX-код цвета точки. 
                /// </summary>
                public string color { get; set; }
            }
        }

        #region VM
        public string VotedCountStr
        {
            get
            {
                int votes = this.votes;
                return UIStringFormatterHelper.FormatNumberOfSomething(votes, "Poll_OneVoteFrm", "Poll_TwoFourVotesFrm","Poll_FiveVotesFrm", true, null, false);
            }
        }

        public string PollTypeStr
        {
            get { return LocalizedStrings.GetString(this.anonymous ? "Poll_AnonymousPoll/Content" : "Poll_PublicPoll"); }
        }

        public string Description
        {
            get { return string.Format("{0} · {1}", this.PollTypeStr, this.VotedCountStr); }
        }

        public string DescriptionBottom
        {
            get { return string.Format("{0} · {1}", this.PollTypeStr, this.VotedCountStr); }
        }

        public Brush BackgroundImage
        {
            get
            {
                if (this.photo != null)
                {
                    ImageBrush b = new ImageBrush();
                    BitmapImage img = new BitmapImage(new Uri(this.photo.images[0].url));
                    b.Stretch = Stretch.Uniform;
                    b.AlignmentY = AlignmentY.Top;
                    b.ImageSource = img;
                    return b;
                }
                else if (this.background != null)
                {
                    if (this.background.type == "gradient")
                    {
                        LinearGradientBrush linear = new LinearGradientBrush();
                        linear.RelativeTransform = new CompositeTransform() { Rotation = this.background.angle, CenterX = 0.5, CenterY = 0.5 };

                        linear.StartPoint = new Point(1, 1);
                        linear.EndPoint = new Point(0, 1);
                        for (int i = 0; i < this.background.points.Count; i++)
                        {
                            var point = this.background.points[i];
                            int offset = i == this.background.points.Count - 1 ? 1 : 0;
                            linear.GradientStops.Add(new GradientStop() { Color = ColorExtensions.FromString("#" + point.color), Offset = offset });
                        }

                        return linear;
                    }
                }
                return null;
            }
        }

        public Visibility BlackVisibility
        {
            get
            {
                return this.photo == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Brush AnswersBackground
        {
            get
            {
                if (this.photo != null)
                {
                    return new SolidColorBrush(ColorExtensions.FromString("#" + photo.color));
                }
                return null;
            }
        }

        public Color AnswersBackgroundColor
        {
            get
            {
                if (this.photo != null)
                {
                    return ColorExtensions.FromString("#" + photo.color);
                }
                return new Color();
            }
        }

        public Color AnswersBackgroundColorTransparent
        {
            get
            {
                if (this.photo != null)
                {
                    Color c = ColorExtensions.FromString("#" + photo.color);
                    c.A = 0;
                    return c;
                }
                return new Color();
            }
        }



        //public ListViewSelectionMode SelectionMode
        //{
        //    get
        //    {
        //        if (this.multiple)
        //            return ListViewSelectionMode.Multiple;
        //        else
        //            return ListViewSelectionMode.None;
        //    }
        //}


        //public Visibility ButtonVisibility
        //{
        //    get
        //    {
        //        if (this.multiple == false)
        //            return Visibility.Collapsed;

        //        return this.answer_ids.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        //    }
        //}
        #endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.owner_id);
            writer.Write(this.id);
            writer.Write(this.created);
            writer.Write(this.closed);
            writer.WriteString(this.question);
            writer.Write(this.votes);
            writer.WriteList(this.answer_ids);
            writer.WriteList(this.answers);
            writer.Write(this.anonymous);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.owner_id = reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.created = reader.ReadDateTime();
            this.closed = reader.ReadBoolean();
            this.question = reader.ReadString();
            this.votes = reader.ReadInt32();
            this.answer_ids = reader.ReadListInt();
            this.answers = reader.ReadList<VKPollAnswers>();
            this.anonymous = reader.ReadBoolean();
        }
    }
}
