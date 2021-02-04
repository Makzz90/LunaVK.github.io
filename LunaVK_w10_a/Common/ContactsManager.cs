using LunaVK.Core;
using LunaVK.Core.DataObjects;
using LunaVK.Core.Library;
using LunaVK.Core.Utils;
using LunaVK.Framework;
using LunaVK.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace LunaVK.Common
{
    public class ContactsManager
    {
        private static readonly string SAVED_CONTACTS_FILE_ID = "SAVED_Contacts";
        private SavedContacts _savedContacts;
        private bool _deleting;
        private bool _synching;
        private bool _synchingPhone;
        private bool _needPersist;

        private static ContactsManager _instance;
        public static ContactsManager Instance
        {
            get
            {
                if (ContactsManager._instance == null)
                    ContactsManager._instance = new ContactsManager();
                return ContactsManager._instance;
            }
        }

        public void EnsureInSyncAsync(bool forceNow = false)
        {
            //
#if DEBUG
            //Test();
#endif
            //
            //IAsyncAction asyncAction = ThreadPool.RunAsync((workItem) =>
            //{
                bool syncContacts = Settings.SyncContacts;
                bool loggedInUser = Settings.IsAuthorized;
                if (!loggedInUser || !syncContacts)
                {
                    this.DeleteAllContactsAsync();
                    return;
                }

                if (!((DateTime.UtcNow - this.GetSavedList().SyncedDate).TotalHours >= 48.0 | forceNow))
                    return;

                UsersService.Instance.GetFriends(0, 0, (res =>
                {
                     if (res.error.error_code == Core.Enums.VKErrors.None)
                     {
                         FriendsCache.Instance.SetFriends(res.response.items);
                         this.SyncContactsAsync(res.response.items);
                     }
                }));
            //});
        }

        private async void Test()
        {
            //ContactPicker picker = new ContactPicker();
            //var c = await picker.PickContactAsync();




            ContactStore contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
            ContactList contactList = null;
            var contactLists = await contactStore.FindContactListsAsync();
            if (contactLists.Count == 0)
                contactList = await contactStore.CreateContactListAsync("LunaVK");
            else
                contactList = contactLists[0];

            ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);
            ContactAnnotationList annotationList;
            var annotationLists = await annotationStore.FindAnnotationListsAsync();
            if (annotationLists.Count > 0)
                annotationList = annotationLists[0];
            else
                annotationList = await annotationStore.CreateAnnotationListAsync();
            //
            //await contactList.DeleteAsync();
            //
            Contact contact = new Contact();
            contact.FirstName = "FirstName";
            contact.LastName = "LastName";
            //contact.MiddleName = "MiddleName";
            contact.RemoteId = "123456";
            //contact.Id ="{1.10003.4}";//"49b0652e-8f39-48c5-853b-e5e94e6b8a11"
            /*
            IDictionary<string, object> dictionary = contact.ProviderProperties;
            dictionary[KnownContactProperties.Url] = "Url";
            dictionary[KnownContactProperties.MobileTelephone] = "MobileTelephone";
            dictionary[KnownContactProperties.AlternateMobileTelephone] = "AlternateMobileTelephone";
            dictionary[KnownContactProperties.Telephone] = "Telephone";
            dictionary[KnownContactProperties.AlternateTelephone] = "AlternateTelephone";
            //dictionary[KnownContactProperties.Birthdate] = new DateTimeOffset(var_8);
            dictionary[KnownContactProperties.WorkAddress] = "WorkAddress";
                */


            ContactPhone phone = new ContactPhone() { Number = "0123456", Kind = ContactPhoneKind.Mobile/*, Description = "Descr"*/ };
            //contact.Phones.Add(phone);
            contact.Nickname = "NickName";

            ContactJobInfo job = new ContactJobInfo() { CompanyName = "CompanyName"/*, Title = "CompanyTitle"*/ };
            //contact.JobInfo.Add(job);

            ContactAnnotation annotation = new ContactAnnotation();
            //annotation.ContactId = contact.Id;
            annotation.RemoteId = contact.RemoteId;
//            annotation.ContactListId = contactList.Id;//BUG
            annotation.SupportedOperations = ContactAnnotationOperations.ContactProfile;// | ContactAnnotationOperations.Message;
            bool temp = await annotationList.TrySaveAnnotationAsync(annotation);


            await contactList.SaveContactAsync(contact);

            /*
            Сведения WinRT: Exactly one of ContactId and ContactListId must be set
            Параметр задан неверно.
            Exactly one of ContactId and ContactListId must be set
            */

        }

        /// <summary>
        /// Удаление всех контактов от этого приложения.
        /// Выполняется на фоновой ветке.
        /// </summary>
        public void DeleteAllContactsAsync()
        {
            if (this._deleting || this._synching)
                return;
            this.DoDeleteAllContactsAsync();
        }

        private void DoDeleteAllContactsAsync()
        {
            Task.Run(async () =>
            {
                this._deleting = true;
                try
                {
                    SavedContacts arg_1E1_0 = this.GetSavedList();
                    arg_1E1_0.SyncedDate = DateTime.MinValue;
                    arg_1E1_0.SavedUsers.Clear();
                    //this.EnsurePersistSavedContactsAsync();

                    ContactStore contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

                    ContactList contactList = null;
                    var contactLists = await contactStore.FindContactListsAsync();
                    if (contactLists.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        contactList = contactLists[0];
                    }
                    await contactList.DeleteAsync();
                    contactList = null;
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error("ContactsManager failed to delete all contacts", ex);
                }
                finally
                {
                    this._deleting = false;
                }
            });
        }

        private SavedContacts GetSavedList()
        {
            if (this._savedContacts == null)
            {
                this._savedContacts = new SavedContacts();
                CacheManager.TryDeserialize(this._savedContacts, ContactsManager.SAVED_CONTACTS_FILE_ID);
            }
            this._needPersist = true;
            return this._savedContacts;
        }

        public void SyncContactsAsync(IReadOnlyList<VKUser> friendsList)
        {
            Task.Run(async () =>
            {
                if (!this._synching && !this._deleting)
                {
                    this._synching = true;
                    SavedContacts savedContacts = this.GetSavedList();

                    Func<VKUser, string> arg_1E7_2 = new Func<VKUser, string>((u) => { return u.id.ToString(); });

                    Func<VKUser, VKUser, bool> arg_1E7_3 = new Func<VKUser, VKUser, bool>((u1, u2) =>
                    {
                        return ContactsManager.AreStringsEqualOrNullEmpty(u1.first_name, u2.first_name) && ContactsManager.AreStringsEqualOrNullEmpty(u1.last_name, u2.last_name) && ContactsManager.AreStringsEqualOrNullEmpty(u1.mobile_phone, u2.mobile_phone) && ContactsManager.AreStringsEqualOrNullEmpty(u1.home_phone, u2.home_phone) && ContactsManager.AreStringsEqualOrNullEmpty(u1.site, u2.site) && ContactsManager.AreStringsEqualOrNullEmpty(u1.bdate, u2.bdate) && ContactsManager.AreStringsEqualOrNullEmpty(u1.photo_max, u2.photo_max);
                    });

                    List<VKUser> list = savedContacts.SavedUsers;
                    List<Tuple<VKUser, VKUser>> updated_users;
                    List<VKUser> new_users;
                    List<VKUser> deleted_users;
                    GetListChanges<VKUser>(list, friendsList, arg_1E7_2, arg_1E7_3, out updated_users, out new_users, out deleted_users);
                    Logger.Instance.Info("ContactsManager got {0} updated users, {1} new users, {2} deleted users", updated_users.Count, new_users.Count, deleted_users.Count);

                    ContactStore contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);
                    //await this.EnsureProvisioned(contactStore);
                    ContactList contactList = null;
                    var contactLists = await contactStore.FindContactListsAsync();
                    if (contactLists.Count == 0)
                        contactList = await contactStore.CreateContactListAsync("LunaVK");
                    else
                        contactList = contactLists[0];

                    ContactAnnotationStore annotationStore = await ContactManager.RequestAnnotationStoreAsync(ContactAnnotationStoreAccessType.AppAnnotationsReadWrite);
                    ContactAnnotationList annotationList;
                    var annotationLists = await annotationStore.FindAnnotationListsAsync();
                    if (annotationLists.Count > 0)
                        annotationList = annotationLists[0];
                    else
                        annotationList = await annotationStore.CreateAnnotationListAsync();

                    //StoredContact storedContact = await contactStore.FindContactByRemoteIdAsync(ContactsManager.GetRemoteId(AppGlobalStateManager.Current.GlobalState.LoggedInUser));
                    //if (storedContact != null)
                    //{
                    //    await this.SetContactProperties(storedContact, AppGlobalStateManager.Current.GlobalState.LoggedInUser, null);
                    //    await storedContact.SaveAsync();
                    //}
                    //contactStore.CreateContactQuery();
#if DEBUG
                    Stopwatch stopwatch = Stopwatch.StartNew();
#endif
                    List<Tuple<VKUser, VKUser>>.Enumerator enumerator = updated_users.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Tuple<VKUser, VKUser> var_8_57F = enumerator.Current;
                        VKUser user = var_8_57F.Item2;
                        VKUser originalUser = var_8_57F.Item1;
                        
                        try
                        {
                            Contact storedContact2 = await contactList.GetContactFromRemoteIdAsync(user.id.ToString());
                            this.SetContactProperties(storedContact2, user, originalUser);


                            if (storedContact2 != null)
                            {
                                ContactAnnotation annotation = new ContactAnnotation();
                                //annotation.ContactId = storedContact2.Id;
                                annotation.RemoteId = storedContact2.RemoteId;
//                                annotation.ContactListId = contactList.Id;//BUG
                                annotation.SupportedOperations = ContactAnnotationOperations.ContactProfile | ContactAnnotationOperations.Message;
                                await annotationList.TrySaveAnnotationAsync(annotation);


                                //await storedContact2.SaveAsync();
                                await contactList.SaveContactAsync(storedContact2);
                            }
                            storedContact2 = null;
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to update contact for user " + user.Title, ex);
                        }
                        user = null;
                        originalUser = null;
                    }
#if DEBUG
                    Debug.WriteLine("Sync updated contacts {0} ms.", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
#endif
                    List<VKUser>.Enumerator enumerator2 = new_users.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        VKUser user2 = enumerator2.Current;
                        try
                        {
                            if (await contactList.GetContactFromRemoteIdAsync(user2.id.ToString()) == null)
                            {
                                //Stopwatch stopwatch = Stopwatch.StartNew();
                                //this.FireSyncStatusChanged(num, count);
                                //Logger.Instance.Info("ContactsManager begin creating user");
                                Contact storedContact3 = new Contact();
                                this.SetContactProperties(storedContact3, user2);
                                /*
                                try
                                {
                                    ContactAnnotation annotation = new ContactAnnotation();
                                    annotation.ContactId = storedContact3.Id;//Value does not fall within the expected range.
                                    annotation.RemoteId = storedContact3.RemoteId;
                                    annotation.SupportedOperations = ContactAnnotationOperations.ContactProfile | ContactAnnotationOperations.Message;
//                                    annotation.ContactListId = contactList.Id;//Fail on old firmware    BUG
                                    await annotationList.TrySaveAnnotationAsync(annotation);//Exactly one of ContactId and ContactListId must be set
                                }
                                catch
                                {

                                }
                                */
                                await contactList.SaveContactAsync(storedContact3);//await storedContact3.SaveAsync();
                                //Logger.Instance.Info("ContactsManager end creating user " + user2.Title);
                                await Task.Delay(500);
                                storedContact3 = null;
                            }
                            list.Add(user2);
                        }
                        catch (Exception var_12_B82)
                        {
                            Logger.Instance.Error("Failed to create contact for user " + user2.Title, var_12_B82);
                        }
                        user2 = null;
                    }

#if DEBUG
                    Debug.WriteLine("Sync new contacts {0} ms.", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
#endif

                    enumerator2 = deleted_users.GetEnumerator();
                    while (enumerator2.MoveNext())
                    {
                        VKUser user3 = enumerator2.Current;
                        
                        try
                        {
                            var var_13_D35 = await contactList.GetContactFromRemoteIdAsync(user3.id.ToString());
                            if (var_13_D35 != null)
                            {
                                await contactList.DeleteContactAsync(var_13_D35);
                            }
                            list.Remove(user3);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error("Failed to delete contact for user " + user3.Title, ex);
                        }
                        user3 = null;
                    }

#if DEBUG
                    Debug.WriteLine("Sync deleted contacts {0} ms.", stopwatch.ElapsedMilliseconds);
                    stopwatch.Restart();
#endif

                    //enumerator2 = default(List<GroupOrUser>.Enumerator);
                    savedContacts.SyncedDate = DateTime.UtcNow;
                    this.EnsurePersistSavedContactsAsync();
                    //savedContacts2 = null;
                    list = null;
                    deleted_users = null;
                    new_users = null;
                    updated_users = null;
                    contactStore = null;
                    //storedContact = null;
                    _synching = false;
                }
            });
        }

        public void EnsurePersistSavedContactsAsync()
        {
            if (!this._needPersist)
                return;
            Logger.Instance.Info("ContactsManager persisting saved contacts data");
            CacheManager.TrySerializeAsync(this._savedContacts, ContactsManager.SAVED_CONTACTS_FILE_ID);
            this._needPersist = false;
        }

        private async Task EnsureProvisioned(ContactStore store)
        {
            /*
            if (Settings.UserId != 0)
            {
                bool flag = true;
                string text = ContactsManager.GetRemoteId(Settings.UserId);
                try
                {
                    if (await store.FindContactByRemoteIdAsync(text) == null)
                    {
                        flag = false;
                    }
                }
                catch (Exception)
                {
                    flag = false;
                }
                if (!flag)
                {
                    try
                    {
                        StoredContact arg_159_0 = await store.CreateMeContactAsync(text);
                        arg_159_0.DisplayName = (Settings.LoggedInUserName);
                        await arg_159_0.SaveAsync();
                    }
                    catch
                    {
                    }
                    try
                    {
                        await SocialManager.ProvisionAsync();
                    }
                    catch
                    {
                    }
                }
                text = null;
            }*/
        }
        /*
        public static string GenerateUniqueRemoteId(string itemId, string itemType)
        {
            return "VK#%^" + itemType + "#%^" + itemId;
        }

        public static string GetRemoteId(VKUser user)
        {
            return GenerateUniqueRemoteId(user.id.ToString(),"UserOrGroup");
        }
        */
        private void SetContactProperties(Contact contact, VKUser user, VKUser originalUser = null)
        {
            if (contact != null)
            {
                contact.RemoteId = user.id.ToString();//contact.RemoteId = GetRemoteId(user);
                contact.Name = user.first_name;
                contact.LastName = user.last_name;
                //contact.Id = user.id.ToString();

                if (!string.IsNullOrWhiteSpace(user.photo_max) && !user.photo_max.Contains("vk.com/images/camera"))
                {
                    var rref = RandomAccessStreamReference.CreateFromUri(new Uri(user.photo_max));
                    contact.SourceDisplayPicture = rref;
                }

                IDictionary<string, object> dictionary = contact.ProviderProperties;
                if (!string.IsNullOrWhiteSpace(user.site))
                {


                    if (!string.IsNullOrEmpty(user.domain) && user.site.Contains(user.domain))
                    {

                    }
                    else
                    {
                        contact.Websites.Add(new ContactWebsite() { RawValue = user.site });
                    }
                }

                if (!string.IsNullOrWhiteSpace(user.mobile_phone) && this.IsPhoneNumber(user.mobile_phone))
                {
                    List<string> var_6_262 = BaseFormatterHelper.ParsePhoneNumbers(user.mobile_phone);
                    if (var_6_262.Count >= 1)
                    {
                        ContactPhone phone = new ContactPhone() { Number = var_6_262[0], Kind = ContactPhoneKind.Mobile };
                        contact.Phones.Add(phone);
                    }
                    if (var_6_262.Count >= 2)
                    {
                        ContactPhone phone = new ContactPhone() { Number = var_6_262[1], Kind = ContactPhoneKind.Mobile };
                        contact.Phones.Add(phone);
                    }
                }

                if (!string.IsNullOrWhiteSpace(user.home_phone) && this.IsPhoneNumber(user.home_phone))
                {
                    List<string> var_7_2D8 = BaseFormatterHelper.ParsePhoneNumbers(user.home_phone);
                    if (var_7_2D8.Count >= 1)
                    {
                        ContactPhone phone = new ContactPhone() { Number = var_7_2D8[0], Kind = ContactPhoneKind.Home };
                        contact.Phones.Add(phone);
                    }
                    if (var_7_2D8.Count >= 2)
                    {
                        ContactPhone phone = new ContactPhone() { Number = var_7_2D8[1], Kind = ContactPhoneKind.Home };
                        contact.Phones.Add(phone);
                    }
                }

                DateTime var_8;
                if (!string.IsNullOrWhiteSpace(user.bdate) && ContactsManager.TryParseDateTimeFromString(user.bdate, out var_8))
                {
                    //var_8 = var_8.Add(DateTime.Now - DateTime.UtcNow);
                    //new DateTimeOffset(var_8.Year, var_8.Month, var_8.Day, 0, 0, 0, 0, TimeSpan.Zero);
                    //dictionary[KnownContactProperties.Birthdate] = new DateTimeOffset(var_8);
                    ContactField field = new ContactField(user.bdate, ContactFieldType.ImportantDate);
                    contact.Fields.Add(field);
                }

                if (user.occupation != null)
                {
                    if (user.occupation.type == VKOccupation.OccupationType.work)
                    {
                        ContactJobInfo job = new ContactJobInfo() { CompanyName = user.occupation.name };
                        contact.JobInfo.Add(job);
                    }
                }

                string prefix = "https://";

                if (CustomFrame.Instance.IsDevicePhone)
                    prefix += "m.";
                prefix += "vk.com/";
                if (string.IsNullOrEmpty(user.domain))
                    prefix += ("id" + user.id);
                else
                    prefix += user.domain;
                contact.Websites.Add(new ContactWebsite() { RawValue = prefix });

                if (!string.IsNullOrEmpty(user.domain))
                {
                    string input = user.domain;
                    if (input != ("id" + user.id))
                    {
                        user.domain = input.First().ToString().ToUpper() + input.Substring(1);
                        contact.Nickname = user.domain;//contact.MiddleName = user.domain;
                    }
                }

            }

            if (originalUser != null)
            {
                originalUser.first_name = user.first_name;
                originalUser.last_name = user.last_name;
                originalUser.site = user.site;
                originalUser.mobile_phone = user.mobile_phone;
                originalUser.home_phone = user.home_phone;
                originalUser.photo_max = user.photo_max;
                originalUser.bdate = user.bdate;
            }
        }

        public static bool AreStringsEqualOrNullEmpty(string str1, string str2)
        {
            if (str1 == null && str2 == string.Empty || str2 == null && str1 == string.Empty)
                return true;
            return str1 == str2;
        }

        public static void GetListChanges<T>(IReadOnlyList<T> originalList, IReadOnlyList<T> updatedList, Func<T, string> getKey, Func<T, T, bool> comparer, out List<Tuple<T, T>> updatedItems, out List<T> newItems, out List<T> deletedItems)
        {
            updatedItems = new List<Tuple<T, T>>();
            newItems = new List<T>();
            deletedItems = new List<T>();
            ILookup<string, T> lookup1 = originalList.ToLookup<T, string>(getKey);
            ILookup<string, T> lookup2 = updatedList.ToLookup<T, string>(getKey);
            foreach (T updated in updatedList)
            {
                IEnumerable<T> source = lookup1[getKey(updated)];
                if (source.Any<T>())
                {
                    T obj = source.First<T>();
                    if (!comparer(updated, obj))
                        updatedItems.Add(new Tuple<T, T>(obj, updated));
                }
                else
                    newItems.Add(updated);
            }
            foreach (T original in originalList)
            {
                if (!lookup2[getKey(original)].Any<T>())
                    deletedItems.Add(original);
            }
        }

        private bool IsPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                return false;
            }
            Func<char, bool> arg_2A_1 = new Func<char, bool>((c) => char.IsDigit(c));

            return Enumerable.Any<char>(phone, arg_2A_1);
        }

        public static bool TryParseDateTimeFromString(string date, out System.DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            try
            {
                string[] strArray = ((string)date).Split((char[])new char[1] { '.' });
                if (strArray.Length != 3)
                    return false;
                int day = int.Parse(strArray[0]);
                int month = int.Parse(strArray[1]);
                int year = int.Parse(strArray[2]);
                dateTime = new DateTime(year, month, day);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("ContactsManager failed to parse date", ex);
                return false;
            }
        }

        public void Sync(Action callback = null)
        {
            Task.Run(async () =>
            {
                if (!Settings.AllowSendContacts || this._synchingPhone)
                {
                    if (callback != null)
                        callback.Invoke();
                }
                else
                {
                    this._synchingPhone = true;
                    List<string> phoneContactsAsync = await GetPhoneContactsAsync();
                    if (phoneContactsAsync.IsNullOrEmpty())
                    {
                        if (callback != null)
                            callback.Invoke();

                        this._synchingPhone = false;
                    }
                    else
                    {
                        ContactsCache contactsCache = RetrieveContactsAsync();
                        if (!AreListsEqual(phoneContactsAsync, contactsCache.PhoneNumbers))
                        {
                            AccountService.Instance.LookupContacts("phone", "", phoneContactsAsync, (result) =>
                            {
                                if (result.error.error_code == Core.Enums.VKErrors.None)
                                {
                                    StoreContactsAsync(new ContactsCache { PhoneNumbers = phoneContactsAsync });
                                }
                            });
                        }
                        this._synchingPhone = false;

                        if (callback != null)
                            callback.Invoke();
                    }
                }
            });
        }

        private bool AreListsEqual(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }
            list1.Sort();
            list2.Sort();
            return !Enumerable.Any<string>(Enumerable.Where<string>(list1, (string t, int i) => t != list2[i]));
        }

        private ContactsCache RetrieveContactsAsync()
        {
            ContactsCache contactsCache = new ContactsCache();
            CacheManager.TryDeserialize(contactsCache, "SAVED_ContactsPhoneNumbers", false);
            return contactsCache;
        }

        private void StoreContactsAsync(ContactsCache contactsCache)
        {
            CacheManager.TrySerialize(contactsCache, "SAVED_ContactsPhoneNumbers", false);
        }

        private async Task<List<string>> GetPhoneContactsAsync()
        {
            List<string> result = new List<string>();

            ContactStore contactStore = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AllContactsReadOnly);

            var contacts = await contactStore.FindContactsAsync();
            foreach (var contact in contacts)
            {
                if (contact.Phones != null)
                    result.AddRange(Enumerable.Select(contact.Phones, (c => c.Number)));
            }

            return result;
        }
    }
}
