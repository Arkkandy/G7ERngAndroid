using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Gen7EggRNG
{
    [Activity(Label = "EggSeedFinderActivity")]
    public class EggSeedFinderActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SeedFinderLayout);

            // Create your application here
            /*TabHost tHost = (TabHost)FindViewById(Resource.Id.seedTypeTab);

            TabHost.TabSpec tab1 = tHost.NewTabSpec("127 Magikarp");
            TabHost.TabSpec tab2 = tHost.NewTabSpec("8 Egg Finder");
            //TabHost.TabSpec tab3 = tHost.NewTabSpec("Seed Recovery");

            tab1.SetIndicator("Tab1");
            tab1.SetContent( new Intent(this,typeof(MagikarpSeedActivity)));

            tab2.SetIndicator("Tab2");
            tab2.SetContent(new Intent(this, typeof(TinyFinderActivity)));

            //tab3.SetIndicator("Tab3");
            //tab2.SetContent(new Intent(this, typeof(SeedRecoveryActivity)));

            tHost.AddTab(tab1);
            tHost.AddTab(tab2);
            //tHost.AddTab(tab3);*/
        }
    }
}