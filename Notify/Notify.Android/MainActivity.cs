using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Provider;
using System.Collections.Generic;
using System.Collections;
using Android.Support.V4.Content;
using Android;
using Android.Util;
using Android.Support.Design.Widget;
using static Android.Resource;
using Android.Support.V4.App;
using System.Threading.Tasks;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Android.Nfc;

namespace Notify.Droid
{
    [Activity(Label = "Notify", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        bool permsGranted = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            accessContacts();


        }

        [Obsolete]
        private void accessContacts()
        {
            TryGetLocationAsync();  
            //neeed to wait here to make sure we have permissiosn before going on.
            if (permsGranted)
            {
                ContactService_Android myCOntacts = new ContactService_Android();
                var contacts = myCOntacts.GetAllContacts();
                return;
            }

        }

        [Obsolete]
        private void TryGetLocationAsync()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                permsGranted = true;
                //return;
            }
            else
            {
                checkForContactsPermissions();
            }

        }



        private static string[] PERMISSIONS_CONTACT = {Manifest.Permission.ReadContacts, Manifest.Permission.WriteContacts};
        private static int REQUEST_CONTACTS = 1;

        [Obsolete]
        //this is async so we need to wait for user respone
        private void checkForContactsPermissions()
        {
            // Verify that all required contact permissions have been granted.
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts)
                    != Permission.Granted
                    || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts)
                    != Permission.Granted)
            {
                //given that Manifest.permission.READ_CONTACTS and Manifest.permission.WRITE_CONTACTS
                //are from the same permission group , on theory it is enough to check for only one of them
                //yet permission groups are subject to change therefore it is s good idea to check
                //for both permissions

                // Contacts permissions have not been granted.
                //Log.i(TAG, "Contact permissions has NOT been granted. Requesting permissions.");

                var activity = (Activity)Forms.Context;  //this call is obsolete...
                ActivityCompat.RequestPermissions(activity, PERMISSIONS_CONTACT, REQUEST_CONTACTS);
                permsGranted = true;
            }
            else
            {
                // Contact permissions have been granted. Show the contacts fragment.
                //Log.i(TAG,
                //        "Contact permissions have already been granted. Displaying contact details.");
                //logic for contacts goes here
                permsGranted = true;
            }

        }
      
        public class PhoneContact
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string PhoneNumber { get; set; }

            public string Name { get => $"{FirstName} {LastName}"; }

        }

        public class ContactService_Android
        {
            public IEnumerable<PhoneContact> GetAllContacts()
            {
                var phoneContacts = new List<PhoneContact>();

                using (var phones = Android.App.Application.Context.ContentResolver.Query(ContactsContract.CommonDataKinds.Phone.ContentUri, null, null, null, null))
                {
                    if (phones != null)
                    {
                        while (phones.MoveToNext())
                        {
                            try
                            {
                                string name = phones.GetString(phones.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName));
                                string phoneNumber = phones.GetString(phones.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));

                                string[] words = name.Split(' ');
                                var contact = new PhoneContact();
                                contact.FirstName = words[0];
                                if (words.Length > 1)
                                    contact.LastName = words[1];
                                else
                                    contact.LastName = ""; //no last name
                                contact.PhoneNumber = phoneNumber;
                                phoneContacts.Add(contact);
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                                //something wrong with one contact, may be display name is completely empty, decide what to do
                            }
                        }
                        phones.Close();
                    }
                    // if we get here, we can't access the contacts. Consider throwing an exception to display to the user
                }

                return phoneContacts;
            }
        }
    }
}
