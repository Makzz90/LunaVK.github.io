using Windows.UI.Xaml;
using LunaVK.Core.ViewModels;
using LunaVK.Core.Framework;
using System.IO;
using LunaVK.Core.Utils;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Массив, содержащий объекты с вариантами ответа на вопрос в опросе.
    /// </summary>
    public sealed class VKPollAnswers : ViewModelBase, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор ответа на вопрос.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Текст ответа на вопрос.
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Количество пользователей, проголосовавших за данный вариант ответа.
        /// </summary>
        public int votes { get; set; }

        /// <summary>
        /// Рейтинг данного варинта ответа, выраженный в процентах.
        /// </summary>
        public double rate { get; set; }

#region VM
        public VKPoll _pollVM;

        public Visibility VotedCheckVisibility
        {
            get
            {
                if (this._pollVM.answer_ids == null || ( this._pollVM.answer_ids.Count == 0 || !this._pollVM.answer_ids.Contains(this.id)))
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public string PercentageStr
        {
            get
            {
                if (this._pollVM.answer_ids.Count == 0 && this._pollVM.can_vote)
                    return "";
                return this.PercentageRelativeValue.ToString("0") + " %";
            }
        }

        public string VotesStr
        {
            get
            {
                if (this._pollVM.answer_ids.Count == 0 && this._pollVM.can_vote)
                    return "";
                return this.votes.ToString();
            }
        }

        private double PercentageRelativeValue
        {
            get
            {
                if (this._pollVM.answer_ids.Count == 0 && this._pollVM.can_vote)
                    return 0.0;
                return this.GetRelativeRate();
            }
        }

        private double GetRelativeRate()
        {
            int votes = this._pollVM.votes;
            if (votes == 0)
                return 0.0;
            return (double)this.votes * 100.0 / (double)votes;
        }

        public double PercentageValue
        {
            get
            {
                if (this._pollVM.answer_ids == null || ( this._pollVM.answer_ids.Count == 0 && this._pollVM.can_vote))
                    return 0.0;
                return this.GetAbsoluteRate();
            }
        }

        private double GetAbsoluteRate()
        {
            //List<VKPollAnswers> answers = this._pollVM.answers;
            //int _maxVotes = answers.Max(a => a.votes);
            int _maxVotes = this._pollVM.votes;

            if (_maxVotes == 0)
                return 0.0;
            return (double)this.votes * 100.0 / (double)_maxVotes;
        }

        /// <summary>
        /// Обновляем интерфейс
        /// </summary>
        public void ReadData()
        {
            //this.NotifyPropertyChanged<string>(() => this.AnswerStr);
            this.NotifyPropertyChanged<string>(() => this.VotesStr);
            this.NotifyPropertyChanged<double>(() => this.PercentageValue);
            this.NotifyPropertyChanged<string>(() => this.PercentageStr);
            this.NotifyPropertyChanged<Visibility>(() => this.VotedCheckVisibility);
        }

        public Visibility MultiVisibility
        {
            get
            {
                return (this._pollVM.multiple  && this._pollVM.answer_ids == null || this._pollVM.answer_ids.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        public void Write(BinaryWriter writer)
        {
            writer.Write(1);
            writer.Write(this.id);
            writer.WriteString(this.text);
            writer.Write(this.votes);
            writer.Write(this.rate);
        }

        public void Read(BinaryReader reader)
        {
            reader.ReadInt32();
            this.id = reader.ReadInt32();
            this.text = reader.ReadString();
            this.votes = reader.ReadInt32();
            this.rate = reader.ReadDouble();
        }
    }
}
