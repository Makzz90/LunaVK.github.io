using Newtonsoft.Json;
using LunaVK.Core.Json;
using System.Collections.Generic;
using LunaVK.Core.Enums;
using System;
using LunaVK.Core.Utils;
using System.IO;
using LunaVK.Core.Framework;

namespace LunaVK.Core.DataObjects
{
    /// <summary>
    /// Базовый профиль пользователя
    /// </summary>
    public class VKUser : VKBaseDataForGroupOrUser, IBinarySerializable
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public uint id { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string first_name { get; set; }

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string last_name { get; set; }

        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VKIsDeactivated deactivated { get; set; }

        /// <summary>
        /// содержимое поля «О себе» из профиля.
        /// </summary>
        public string about { get; set; }


        public string activity { get; set; }//нет в документации

        /// <summary>
        /// содержимое поля «Деятельность» из профиля. 
        /// </summary>
        public string activities { get; set; }//есть в документации, но не работает

        /// <summary>
        /// дата рождения. Возвращается в формате D.M.YYYY
        /// или D.M (если год рождения скрыт).
        /// Если дата рождения скрыта целиком, поле отсутствует в ответе. 
        /// </summary>
        public string bdate { get; set; }

        /// <summary>
        /// находится ли текущий пользователь в черном списке
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool blacklisted { get; set; }

        /// <summary>
        /// находится ли пользователь в черном списке у текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool blacklisted_by_me { get; set; }

        /// <summary>
        /// содержимое поля «Любимые книги» из профиля пользователя. 
        /// </summary>
        public string books { get; set; }

        /// <summary>
        /// может ли текущий пользователь видеть профиль при is_closed = 1 (например, если он есть в друзьях). 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_access_closed { get; set; }

        /// <summary>
        /// информация о том, может ли текущий пользователь оставлять записи на стене
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_post { get; set; }

        /// <summary>
        /// информация о том, может ли текущий пользователь видеть чужие записи на стене
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_see_all_posts { get; set; }

        /// <summary>
        /// информация о том, может ли текущий пользователь видеть аудиозаписи
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_see_audio { get; set; }

        /// <summary>
        /// информация о том, будет ли отправлено уведомление
        /// пользователю о заявке в друзья от текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_send_friend_request { get; set; }


        /// <summary>
        /// Могу ли я наптсать сообщение пользователю
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_write_private_message { get; set; }

        /// <summary>
        /// информация о карьере пользователя
        /// </summary>
        public Career career { get; set; }

        /// <summary>
        /// информация о городе, указанном на странице пользователя в разделе «Контакты»
        /// </summary>
        public VKCity city { get; set; }

        /// <summary>
        /// количество общих друзей с текущим пользователем.
        /// </summary>
        public int common_count { get; set; }
        //connections //возвращает данные об указанных в профиле сервисах пользователя, таких как: skype, facebook, twitter, livejournal, instagram. 
        //contacts-вместо него возвращаются мобильный и домашний телефоны

        /// <summary>
        /// количество различных объектов у пользователя
        /// </summary>
        public VKCounters counters { get; set; }

        /// <summary>
        /// информация о стране, указанной на странице пользователя в разделе «Контакты»
        /// </summary>
        public VKCountry country { get; set; }

        /// <summary>
        /// возвращает данные о точках, по которым вырезаны профильная и миниатюрная фотографии пользователя. 
        /// </summary>
        public CropPhoto crop_photo { get; set; }

        /// <summary>
        /// короткий адрес страницы
        /// </summary>
        public string domain { get; set; }

        /// <summary>
        /// информация о высшем учебном заведении пользователя.
        /// </summary>
        public Education education { get; set; }

        /// <summary>
        /// внешние сервисы, в которые настроен экспорт из ВК (twitter, facebook, livejournal, instagram). 
        /// </summary>
        public Exports exports { get; set; }

        /*
         * имя в заданном падеже. Возможные значения для {case}:
        •nom — именительный; 
        •gen — родительный; 
        •dat — дательный; 
        •acc — винительный; 
        •ins — творительный; 
        •abl — предложный.
         * */

        /// <summary>
        /// именительный
        /// </summary>
        public string first_name_nom { get; set; }

        /// <summary>
        /// родительный
        /// </summary>
        public string first_name_gen { get; set; }

        /// <summary>
        /// дательный
        /// </summary>
        public string first_name_dat { get; set; }

        /// <summary>
        /// винительный
        /// </summary>
        public string first_name_acc { get; set; }

        /// <summary>
        /// творительный
        /// </summary>
        public string first_name_ins { get; set; }

        /// <summary>
        /// предложный
        /// </summary>
        public string first_name_abl { get; set; }


        public string last_name_nom { get; set; }
        public string last_name_gen { get; set; }
        public string last_name_dat { get; set; }
        public string last_name_acc { get; set; }
        public string last_name_ins { get; set; }
        public string last_name_abl { get; set; }

        /// <summary>
        /// количество подписчиков пользователя
        /// </summary>
        public int followers_count { get; set; }

        /// <summary>
        /// статус дружбы с пользователем
        /// </summary>
        public VKUsetMembershipType friend_status { get; set; }

        /// <summary>
        /// содержимое поля «Любимые игры» из профиля
        /// </summary>
        public string games { get; set; }

        /// <summary>
        /// известен ли номер мобильного телефона пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool has_mobile { get; set; }

        /// <summary>
        /// 1, если пользователь установил фотографию для профиля. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool has_photo { get; set; }

        /// <summary>
        /// название родного города
        /// </summary>
        public string home_town { get; set; }

        /// <summary>
        /// содержимое поля «Интересы» из профиля
        /// </summary>
        public string interests { get; set; }

        /// <summary>
        /// есть ли пользователь в закладках у текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_favorite { get; set; }

        /// <summary>
        /// информация о том, является ли пользователь другом текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_friend { get; set; }

        /// <summary>
        /// информация о том, скрыт ли пользователь из ленты новостей текущего пользователя
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_hidden_from_feed { get; set; }

        /// <summary>
        /// Время последнего посещения ВКонтакте.
        /// </summary>
        public VKLastSeen last_seen { get; set; }

        /// <summary>
        /// разделенные запятой идентификаторы списков друзей, в которых состоит пользователь.
        /// Поле доступно только для метода friends.get. 
        /// </summary>
        public List<int> lists { get; set; }

        /// <summary>
        /// девичья фамилия. 
        /// </summary>
        public string maiden_name { get; set; }

        /// <summary>
        /// информация о военной службе пользователя.
        /// </summary>
        public Military military { get; set; }

        /// <summary>
        /// содержимое поля «Любимые фильмы» из профиля пользователя. 
        /// </summary>
        public string movies { get; set; }

        /// <summary>
        /// содержимое поля «Любимая музыка» из профиля пользователя. 
        /// </summary>
        public string music { get; set; }

        /// <summary>
        /// никнейм (отчество) пользователя. 
        /// </summary>
        public string nickname { get; set; }

        /// <summary>
        /// информация о текущем роде занятия пользователя
        /// </summary>
        public VKOccupation occupation { get; set; }

        /// <summary>
        /// Находится ли пользователь на сайте (ПК)
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool online { get; set; }

        /// <summary>
        /// Зашел ли пользователь с мобильного устройства на сайт
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool online_mobile { get; set; }

        /*
         * При этом, если используется именно приложение, дополнительно возвращается поле online_app, содержащее его идентификатор.
         * */

        public string online_app { get; set; }

        public UserPersonal personal { get; set; }


        public string photo_50 { get; set; }

        /// <summary>
        /// url квадратной фотографии пользователя, имеющей ширину 100 пикселей
        /// </summary>
        public string photo_100 { get; set; }

        public string photo_100_orig { get; set; }

        /// <summary>
        /// url квадратной фотографии, имеющей ширину 200 пикселей
        /// </summary>
        public string photo_200 { get; set; }

        /// <summary>
        /// url фотографии пользователя, имеющей ширину 200 пикселей
        /// </summary>
        public string photo_200_orig { get; set; }

        /// <summary>
        /// url фотографии, имеющей ширину 400 пикселей.
        /// Если у пользователя отсутствует фотография такого размера, в ответе вернется 
        /// </summary>
        public string photo_400_orig { get; set; }

        /// <summary>
        /// строковый идентификатор главной фотографии профиля
        /// пользователя в формате {user_id}_{photo_id}, например,
        /// 6492_192164258. Обратите внимание, это поле может отсутствовать в ответе. 
        /// </summary>
        public string photo_id { get; set; }

        /// <summary>
        /// url квадратной фотографии с максимальной шириной
        /// </summary>
        public string photo_max { get; set; }

        public string photo_max_orig { get; set; }

        /// <summary>
        /// любимые цитаты. 
        /// </summary>
        public string quotes { get; set; }

        /// <summary>
        /// список родственников
        /// </summary>
        public List<Relative> relatives { get; set; }

        /// <summary>
        /// семейное положение
        /// </summary>
        public RelationshipStatus relation { get; set; }

        public List<School> schools { get; set; }

        /// <summary>
        /// короткое имя страницы. 
        /// </summary>
        public string screen_name { get; set; }

        /// <summary>
        /// Пол пользователя.
        /// </summary>
        public VKUserSex sex { get; set; }

        /// <summary>
        /// адрес сайта, указанный в профиле. 
        /// </summary>
        public string site { get; set; }

        /// <summary>
        /// Статус пользователя.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// временная зона.
        /// Только при запросе информации о текущем пользователе. 
        /// </summary>
        public int timezone { get; set; }

        /// <summary>
        /// скрыт ли профиль пользователя настройками приватности. 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_closed { get; set; }

        /// <summary>
        /// Пользователь подписан на уведомления?
        /// нет в документации
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool is_subscribed { get; set; }

        /// <summary>
        /// информация о том, есть ли на странице пользователя «огонёк». 
        /// </summary>
        [JsonConverter(typeof(VKBooleanConverter))]
        public bool trending { get; set; }

        /// <summary>
        /// любимые телешоу. 
        /// </summary>
        public string tv { get; set; }

        public List<University> universities { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool verified { get; set; }

        /// <summary>
        /// режим стены по умолчанию. Возможные значения: owner, all. 
        /// </summary>
        public string wall_default { get; set; }

        /// <summary>
        /// номер мобильного телефона пользователя (только для Standalone-приложений); 
        /// </summary>
        public string mobile_phone { get; set; }

        /// <summary>
        /// дополнительный номер телефона пользователя. 
        /// </summary>
        public string home_phone { get; set; }

        /// <summary>
        /// уровень полномочий руководителя
        /// </summary>
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public CommunityManagementRole role { get; set; }

        public BlockInformation ban_info { get; set; }

        [JsonConverter(typeof(VKBooleanConverter))]
        public bool can_see_gifts{ get; set; }

    #region Classes

    public sealed class BlockInformation
        {
            /// <summary>
            /// идентификатор администратора, который добавил пользователя
            /// или сообщество в черный список. 
            /// </summary>
            public int admin_id { get; set; }

            /// <summary>
            /// причина добавления в черный список
            /// 0 — другое (по умолчанию); 
            /// •1 — спам; 
            /// •2 — оскорбление участников; 
            /// •3 — нецензурные выражения; 
            /// •4 — сообщения не по теме.
            /// </summary>
            public int reason { get; set; }

            /// <summary>
            /// текст комментария
            /// </summary>
            public string comment { get; set; }

            //public int comment_visible { get; set; }

            /// <summary>
            /// дата добавления в черный список в формате Unixtime
            /// </summary>
            [JsonConverter(typeof(UnixtimeToDateTimeConverter))]
            public DateTime date { get; set; }

            /// <summary>
            /// дата окончания блокировки (0 — блокировка вечная). 
            /// </summary>
            public int end_date { get; set; }

            #region VM
            public VKUser Manager { get; set; }
            #endregion
        }

        public sealed class Exports
        {
            public int twitter { get; set; }

            public int facebook { get; set; }

            public int livejournal { get; set; }

            public int instagram { get; set; }
        }

        public sealed class Education
        {
            public int university { get; set; }

            public string university_name { get; set; }

            public int faculty { get; set; }

            public string faculty_name { get; set; }

            public int graduation { get; set; }
        }

        public class Career
        {
            /// <summary>
            /// идентификатор сообщества (если доступно, иначе company); 
            /// </summary>
            public int group_id { get; set; }

            /// <summary>
            /// название компании (если доступно, иначе group_id); 
            /// </summary>
            public string company { get; set; }

            /// <summary>
            /// идентификатор страны; 
            /// </summary>
            public int country_id { get; set; }

            /// <summary>
            /// идентификатор города (если доступно, иначе city_name); 
            /// </summary>
            public int city_id { get; set; }

            /// <summary>
            /// название города (если доступно, иначе city_id); 
            /// </summary>
            public string city_name { get; set; }

            /// <summary>
            /// год начала работы; 
            /// </summary>
            public int from { get; set; }

            /// <summary>
            /// год окончания работы; 
            /// </summary>
            public int until { get; set; }

            /// <summary>
            /// должность. 
            /// </summary>
            public string position { get; set; }
        }

        public class Military
        {
            /// <summary>
            /// номер части; 
            /// </summary>
            public string unit { get; set; }

            /// <summary>
            /// идентификатор части в базе данных; 
            /// </summary>
            public int unit_id { get; set; }

            /// <summary>
            /// идентификатор страны, в которой находится часть; 
            /// </summary>
            public int country_id { get; set; }

            /// <summary>
            /// год начала службы; 
            /// </summary>
            public int from { get; set; }

            /// <summary>
            /// год окончания службы. 
            /// </summary>
            public int until { get; set; }
        }

        public class UserPersonal
        {
            /// <summary>
            /// политические предпочтения
            /// </summary>
            public int political { get; set; }

            /// <summary>
            /// языки
            /// </summary>
            public List<string> langs { get; set; }

            /// <summary>
            /// мировоззрение
            /// </summary>
            public string religion { get; set; }

            /// <summary>
            /// источники вдохновения. 
            /// </summary>
            public string inspired_by { get; set; }

            /// <summary>
            /// главное в людях
            /// </summary>
            public int people_main { get; set; }

            /// <summary>
            /// главное в жизни
            /// </summary>
            public int life_main { get; set; }

            /// <summary>
            /// отношение к курению
            /// </summary>
            public int smoking { get; set; }

            /// <summary>
            /// отношение к алкоголю.
            /// </summary>
            public int alcohol { get; set; }
        }

        public class Relative
        {
            /// <summary>
            /// идентификатор пользователя; 
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// имя родственника (в том случае, если родственник
            /// не является пользователем ВКонтакте, в этом случае id не возвращается); 
            /// </summary>
            public string name { get; set; }

            /// <summary>
            ///  тип родственной связи.
            /// </summary>
            public string type { get; set; }
        }

        public class School
        {
            /// <summary>
            /// идентификатор школы; 
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// идентификатор страны, в которой расположена школа; 
            /// </summary>
            public int country { get; set; }

            /// <summary>
            /// идентификатор города, в котором расположена школа; 
            /// </summary>
            public int city { get; set; }

            /// <summary>
            /// наименование школы 
            /// </summary>
            public string name { get; set; }

            /// <summary>
            ///  год начала обучения; 
            /// </summary>
            public int year_from { get; set; }

            /// <summary>
            /// год окончания обучения; 
            /// </summary>
            public int year_to { get; set; }

            /// <summary>
            /// год выпуска; 
            /// </summary>
            public int year_graduated { get; set; }

            /// <summary>
            ///  буква класса; 
            /// </summary>
//            public string @class { get; set; }

            /// <summary>
            /// специализация; 
            /// </summary>
            public string speciality { get; set; }

            /// <summary>
            /// идентификатор типа; 
            /// </summary>
            public int type { get; set; }

            /// <summary>
            /// название типа
            /// </summary>
            public string type_str { get; set; }
        }

        public class University
        {
            /// <summary>
            /// идентификатор университета; 
            /// </summary>
            public int id { get; set; }

            /// <summary>
            /// идентификатор страны, в которой расположен университет; 
            /// </summary>
            public int country { get; set; }

            public int city { get; set; }

            /// <summary>
            ///  наименование университета; 
            /// </summary>
            public string name { get; set; }

            /// <summary>
            /// идентификатор факультета; 
            /// </summary>
            public int faculty { get; set; }

            /// <summary>
            /// наименование факультета; 
            /// </summary>
            public string faculty_name { get; set; }

            /// <summary>
            /// идентификатор кафедры; 
            /// </summary>
            public int chair { get; set; }

            public string chair_name { get; set; }

            /// <summary>
            /// год окончания обучения; 
            /// </summary>
            public int graduation { get; set; }

            /// <summary>
            ///  форма обучения; 
            /// </summary>
            public string education_form { get; set; }

            /// <summary>
            /// статус (например, «Выпускник (специалист)»). 
            /// </summary>
            public string education_status { get; set; }
        }

        public class Partner
        {
            public int id { get; set; }

            public string first_name { get; set; }

            public string last_name { get; set; }






            public string acc { get; set; }

            public string ins { get; set; }

            public string abl { get; set; }
        }
        #endregion
        public Partner relation_partner { get; set; }

#region VM
        public string NameAcc
        {
            get { return this.first_name_acc + " " + this.last_name_acc; }
        }

        public string NameDat
        {
            get { return this.first_name_dat + " " + this.last_name_dat; }
        }

        public string NameLink
        {
            get
            {
                return string.Format("[id{0}|{1}]", this.id, this.Title);
            }
        }

        public bool IsFemale
        {
            get { return this.sex == VKUserSex.Female; }
        }

        public string MinPhoto
        {
            get
            {
                string str = this.photo_50;
                if (string.IsNullOrEmpty(str))
                    str = this.photo_100;
                if (string.IsNullOrEmpty(str))
                    str = this.photo_200;
                if (string.IsNullOrEmpty(str))
                    str = this.photo_max;
                return str;
            }
        }

        /// <summary>
        /// Полное имя пользователя.
        /// </summary>
        public string Title
        {
            get { return this.first_name + " " + this.last_name; }
        }

        public string PlatformIcon
        {
            get
            {
                if (this.online == false)
                    return "";

                if (this.last_seen == null)
                    return "\xF137";

                switch (this.last_seen.platform)
                {
                    case VKPlatform.Mobile:
                        {
                            return "\xEE64";
                        }
                    case VKPlatform.Web:
                        {
                            return "\xF137";
                        }
                    case VKPlatform.iPad:
                    case VKPlatform.iPhone:
                        {
                            return "\xEE77";
                        }
                    case VKPlatform.Android:
                        {
                            return "\xEE79";
                        }
                    case VKPlatform.Windows:
                    case VKPlatform.WindowsPhone:
                        {
                            return "\xEE63";
                        }
                    default:
                        {
#if DEBUG
                            //                            System.Diagnostics.Debug.WriteLine(this.last_seen.platform.ToString());
#endif
                            break;

                        }
                }
                return "\xF137";//WEB
            }
        }

        public string Description
        {
            get
            {
                if(this.ban_info!=null)
                {
                    return string.Format("{0} {1}", LocalizedStrings.GetString(this.ban_info.Manager.sex != VKUserSex.Female ? "AddedMale" : "AddedFemale"), this.ban_info.Manager.Title);
                }

                switch (this.role)
                {
                    case CommunityManagementRole.Moderator:
                        return LocalizedStrings.GetString("CommunityManager_Moderator/Text");
                    case CommunityManagementRole.Editor:
                        return LocalizedStrings.GetString("CommunityManager_Editor/Text");
                    case CommunityManagementRole.Administrator:
                        return LocalizedStrings.GetString("CommunityManager_Administrator/Text");
                    case CommunityManagementRole.Creator:
                        return LocalizedStrings.GetString("CommunityManager_Creator/Text");
                    default:
                        {
                            if (this.occupation != null)
                                return this.occupation.name;
                            else if (this.city != null)
                            {
                                string str = this.city.title;
                                int year = UserExtensions.GetBDateYear(this);
                                if (year != 0)
                                {
                                    if (year < DateTime.Now.Year)
                                    {
                                        int years = DateTime.Now.Year - year;
                                        str += ", ";
                                        str += UIStringFormatterHelper.FormatNumberOfSomething(years, "YearsOld_OneFrm", "YearsOld_TwoFourFrm", "YearsOld_FiveFrm", true);
                                    }

                                }

                                return str;
                            }
                            else if (!string.IsNullOrEmpty(this.activity))
                                return this.activity;
                            return "";
                        }
                }
            }
        }

        public string FirstNameLastNameShort
        {
            get
            {
                string str = this.first_name;
                if (!string.IsNullOrWhiteSpace(this.last_name))
                    str = str + " " + this.last_name[0].ToString().ToUpperInvariant() + ".";
                return str;
            }
        }
#endregion

        /// <summary>
        /// +id
        /// </summary>
        public int Id
        {
            get { return (int)this.id; }
        }

        public bool IsVerified
        {
            get { return this.verified; }
        }

        public VKIsDeactivated Deactivated { get { return this.deactivated; } }

        public VKCounters Counters { get { return this.counters; } }

        public bool IsSubscribed
        {
            get { return this.is_subscribed; }
            set { this.is_subscribed = value; }
        }

        public bool IsFavorite
        {
            get { return this.is_favorite; }
            set { this.is_favorite = value; }
        }

        public List<VKUser> randomMutualFriends { get; set; }

        public VKGroup occupationGroup { get; set; }

        /// <summary>
        /// Пользователя связанные отношениями
        /// </summary>
        public List<VKUser> relativesUsers { get; set; }


        public override string ToString()
        {
            return string.Format("{0} {1}", this.id, this.FirstNameLastNameShort);
        }

        public VKGroupMainSection MainSectionType
        {
            get
            {
                return VKGroupMainSection.Photos;
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(2);
            writer.Write(this.id);
            writer.WriteString(this.first_name);
            writer.WriteString(this.last_name);
            //writer.WriteString(this.photo_rec);
            writer.WriteString(this.photo_50);
            writer.WriteString(this.photo_100);
            writer.Write((byte)this.sex);
            //writer.Write<Exports>(this.exports, false);
            writer.WriteString(this.activity);
            writer.WriteString(this.photo_max);
            writer.WriteString(this.site);
            writer.WriteString(this.mobile_phone);
            writer.WriteString(this.home_phone);
            writer.WriteString(this.bdate);
            writer.WriteString(this.first_name_acc);
            writer.WriteString(this.last_name_acc);
            writer.WriteString(this.first_name_gen);
            writer.WriteString(this.last_name_gen);
            writer.WriteString(this.first_name_dat);
            writer.WriteString(this.last_name_dat);
            writer.Write((byte)this.friend_status);
            //writer.Write<BlockInformation>(this.ban_info, false);
            //writer.WriteString(this.role);
            writer.WriteString(this.domain);
            writer.Write<VKLastSeen>(this.last_seen);

            writer.Write(this.verified);
        }

        public void Read(BinaryReader reader)
        {
            int num1 = reader.ReadInt32();
            this.id = reader.ReadUInt32();
            this.first_name = reader.ReadString();
            this.last_name = reader.ReadString();
            //this.photo_rec = reader.ReadString();
            this.photo_50 = reader.ReadString();
            this.photo_100 = reader.ReadString();
            this.sex = (VKUserSex)reader.ReadByte();
            //this.exports = reader.ReadGeneric<Exports>();
            this.activity = reader.ReadString();

            this.photo_max = reader.ReadString();
            this.site = reader.ReadString();
            this.mobile_phone = reader.ReadString();
            this.home_phone = reader.ReadString();
            this.bdate = reader.ReadString();


            this.first_name_acc = reader.ReadString();
            this.last_name_acc = reader.ReadString();


            this.first_name_gen = reader.ReadString();
            this.last_name_gen = reader.ReadString();

            this.first_name_dat = reader.ReadString();
            this.last_name_dat = reader.ReadString();


            this.friend_status = (VKUsetMembershipType)reader.ReadByte();

            //this.ban_info = reader.ReadGeneric<BlockInformation>();

            //this.role = reader.ReadString();

            this.domain = reader.ReadString();
            this.last_seen = reader.ReadGeneric<VKLastSeen>();

            this.verified = reader.ReadBoolean();
        }
    }
}
