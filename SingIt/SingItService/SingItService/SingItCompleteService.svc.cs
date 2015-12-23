using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SingItService
{
    public class DifficultyOrdering
    {
        public string username;
        public string song1, song2;

        //Votes
        public int n1, n2;

        public DifficultyOrdering(string username, string song1, string song2)
        {
            this.username = username;

            this.song1 = song1;
            this.song2 = song2;
        }
    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class SingItCompleteService : ISingItCompleteService
    {
        public string GetRecommendedSongForUser(string username)
        {
            try
            {
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    List<user> allUsers = context.users.ToList();
                    List<song> allSongs = context.songs.ToList();

                    var allSongsGroupedByUser = allSongs.GroupBy(s => s.username).ToList();
                    var allSongsGroupedByTitle = allSongs.GroupBy(s => s.songtitle).ToList();

                    List<DifficultyOrdering> orderings = new List<DifficultyOrdering>();

                    for (int i = 0; i < allSongsGroupedByTitle.Count; i++)
                    {
                        for (int j = 0; j < allSongsGroupedByTitle.Count; j++)
                        {
                            if (i != j)
                            {
                                var allSongsOfSameTitle1 = allSongsGroupedByTitle[i].Select(s => s).ToList();
                                var allSongsOfSameTitle2 = allSongsGroupedByTitle[j].Select(s => s).ToList();

                                for (int ii = 0; ii < allSongsOfSameTitle1.Count; ii++)
                                {
                                    for (int jj = 0; jj < allSongsOfSameTitle2.Count; jj++)
                                    {
                                        if (allSongsOfSameTitle1[ii].username == allSongsOfSameTitle2[jj].username)
                                        {
                                            DifficultyOrdering ordering = null;
                                            var orderingMatch = orderings.Where(o => o.username == allSongsOfSameTitle1[ii].username && o.song1 == allSongsOfSameTitle1[ii].songtitle && o.song2 == allSongsOfSameTitle2[jj].songtitle).ToList();
                                            if (orderingMatch.Count > 0)
                                            {
                                                ordering = orderingMatch.First();
                                            }
                                            else
                                            {
                                                ordering = new DifficultyOrdering(allSongsOfSameTitle1[ii].username, allSongsOfSameTitle1[ii].songtitle, allSongsOfSameTitle2[jj].songtitle);
                                            }

                                            double song1AverageRatings = 0;
                                            List<Rating> song1Ratings = Utilities.JsonDeSerialize<List<Rating>>(allSongsOfSameTitle1[ii].ratings);
                                            if (song1Ratings.Count > 0)
                                            {
                                                song1AverageRatings = song1Ratings.Average(r => r.rating);
                                            }

                                            double song2AverageRatings = 0;
                                            List<Rating> song2Ratings = Utilities.JsonDeSerialize<List<Rating>>(allSongsOfSameTitle2[jj].ratings);
                                            if (song2Ratings.Count > 0)
                                            {
                                                song2AverageRatings = song2Ratings.Average(r => r.rating);
                                            }

                                            if (song1AverageRatings > song2AverageRatings)
                                            {
                                                (ordering.n1)++;
                                            }
                                            else
                                            {
                                                (ordering.n2)++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    var allSongsPermutations =
                        from s1 in Enumerable.Range(0, 1 << allSongs.Count)
                        select
                            from s2 in Enumerable.Range(0, allSongs.Count)
                            where (s1 & (1 << s2)) != 0
                            select allSongs[s2];


                    /*for (int i = 0; i < allSongsGroupedByUser.Count; i++)
                    {
                        var allSongsForCurrentUserGroupedByTitle = allSongsGroupedByUser[i].GroupBy(s => s.songtitle).ToList();
                        for (int j = 0; j < allSongsForCurrentUserGroupedByTitle[i].Count(); j++)
                        {

                        }
                    }

                    for (int i = 0; i < allSongsGroupedByTitle.Count; i++)
                    {
                        List<Rating> userMatchSongRatings = Utilities.JsonDeSerialize<List<Rating>>(userMatchSongs[i].ratings);
                        for (int j = 0; j < allSongsGroupedByTitle[i].Count(); j++)
                        {

                        }
                    }
                    */
                    List<user> userMatch = allUsers.Where(u => u.username == username).ToList();
                    if (userMatch.Count() > 0)
                    {
                        List<song> userMatchSongs = allSongs.Where(s => s.username == username).ToList();
                        if (userMatchSongs.Count > 0)
                        {
                            double[] userMatchSongAverageRatings = new double[userMatchSongs.Count];
                            for (int i = 0; i < userMatchSongs.Count; i++)
                            {
                                List<Rating> userMatchSongRatings = Utilities.JsonDeSerialize<List<Rating>>(userMatchSongs[i].ratings);
                                if (userMatchSongRatings.Count > 0)
                                {
                                    userMatchSongAverageRatings[i] = userMatchSongRatings.Average(r => r.rating);
                                }
                            }
                            double userMatchSongMaxAverageRating = userMatchSongAverageRatings.Max();
                            song userMatchSongWithMaxAverageRating = userMatchSongs[Array.IndexOf(userMatchSongAverageRatings, userMatchSongMaxAverageRating)];

                            List<song> otheruserUnsungSongsWithSameGenre = allSongs.Where(s => s.username != username && s.songgenre == userMatchSongWithMaxAverageRating.songgenre && userMatchSongWithMaxAverageRating.user.songs.Where(us => s.songtitle == us.songtitle).Count() == 0).ToList();
                            double[] otheruserUnsungSongsWithSameGenreAverageRatings = new double[otheruserUnsungSongsWithSameGenre.Count];
                            for (int i = 0; i < otheruserUnsungSongsWithSameGenre.Count; i++)
                            {
                                List<Rating> otheruserUnsungSongsWithSameGenreRatings = Utilities.JsonDeSerialize<List<Rating>>(otheruserUnsungSongsWithSameGenre[i].ratings);
                                if (otheruserUnsungSongsWithSameGenreRatings.Count > 0)
                                {
                                    otheruserUnsungSongsWithSameGenreAverageRatings[i] = otheruserUnsungSongsWithSameGenreRatings.Average(r => r.rating);
                                }
                            }
                            double otheruserUnsungSongsWithSameGenreMaxAverageRating = otheruserUnsungSongsWithSameGenreAverageRatings.Max();
                            song otheruserUnsungSongsWithSameGenreWithMaxAverageRating = otheruserUnsungSongsWithSameGenre[Array.IndexOf(otheruserUnsungSongsWithSameGenreAverageRatings, otheruserUnsungSongsWithSameGenreMaxAverageRating)];

                            return otheruserUnsungSongsWithSameGenreWithMaxAverageRating.songtitle;
                        }
                    }
                }

                return Utilities.FAILURE;
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }

        public string Validate(string username, string password)
        {
            try
            {
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    List<user> usermatch = context.users.Where(u => u.username == username && u.password == password).ToList();
                    if (usermatch.Count() > 0)
                    {
                        return Utilities.JsonSerialize(usermatch.First());
                    }
                    else
                    {
                        return Utilities.FAILURE;
                    }
                }
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }

        public bool CreateUser(string username, string password, string email)
        {
            try
            {
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    context.users.Add(new user()
                    {
                        username = username,
                        password = password,
                        email = email
                    });

                    context.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetAllSongsForUser(string username)
        {
            try
            {
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    return Utilities.JsonSerialize(context.users.Where(u => u.username == username).First().songs);
                }
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }

        public string GetAllSongsGroupedBySongTitle(string username)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"<li data-role=""list-divider""></li>");
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    var AllSongsGroupedBySongTitle = context.songs.GroupBy(s => s.songtitle).ToList();
                    for (int i = 0; i < AllSongsGroupedBySongTitle.Count; i++)
                    {
                        sb.Append(@"<li data-role=""collapsible"" data-iconpos=""right"" data-shadow=""false"" data-corners=""false"">");

                        sb.Append(@"<h2>");
                        sb.Append(AllSongsGroupedBySongTitle[i].Key);
                        sb.Append(@"</h2>");

                        sb.Append(@"<ul data-role=""listview"" data-shadow=""false"" data-inset=""true"" data-corners=""false"">");

                        var AllSongsForCurrentSongTitle = AllSongsGroupedBySongTitle[i].ToList();
                        for (int j = 0; j < AllSongsForCurrentSongTitle.Count; j++)
                        {
                            string currentUserAndSong = (i + 1).ToString() + (j + 1).ToString();
                            string currentData = string.Concat(@"data-songfilename=""", AllSongsForCurrentSongTitle[j].songfilename, @"""");

                            List<Rating> ratingsList = Utilities.JsonDeSerialize<List<Rating>>(AllSongsForCurrentSongTitle[j].ratings);

                            double averageRating = 0;
                            if (ratingsList.Count > 0)
                            {
                                averageRating = ratingsList.Select(s => s.rating).Average();
                            }

                            bool matchFoundForRating = false;
                            int matchRating = 0;
                            List<Rating> ratingsListMatch = ratingsList.Where(r => r.username == username).ToList();
                            if (ratingsListMatch.Count > 0)
                            {
                                matchFoundForRating = true;
                                matchRating = ratingsListMatch.First().rating;
                            }

                            sb.Append(@"<li data-role=""collapsible"" data-iconpos=""right"" data-collapsed-icon=""arrow-r"" data-expanded-icon=""arrow-d"" data-shadow=""false"" data-corners=""false"">");
                            sb.Append(@"<h3>");
                            sb.Append(AllSongsForCurrentSongTitle[j].username);
                            sb.Append(@"</h3>");

                            sb.Append(@"<h5 class=""lv bold-custom"">Play</h5>");

                            sb.Append(@"<audio controls><source src=""");
                            sb.Append(Utilities.BLOB_BASE_URL);
                            sb.Append(@"/");
                            sb.Append(AllSongsForCurrentSongTitle[j].username);
                            sb.Append(@"/");
                            sb.Append(AllSongsForCurrentSongTitle[j].songfilename);
                            sb.Append(@"""></audio>");

                            sb.Append(@"<h5 class=""lv normal-custom"">Rate:</h5>");
                            sb.Append(@"<fieldset data-role=""controlgroup"" data-type=""horizontal"" data-mini=""true"">");

                            for (int k = 0; k < 5; k++)
                            {
                                string currentLabel = (k + 1).ToString();

                                sb.Append(@"<label><input type=""radio"" name=""radio-choice-");
                                sb.Append(currentUserAndSong);
                                sb.Append(@""" id=""radio-choice-");
                                sb.Append(currentUserAndSong);
                                sb.Append(currentLabel);
                                sb.Append(@""" value=""");
                                sb.Append(currentLabel);
                                sb.Append(@""" onclick=""app.ratingsRadioButtonOnClick(event)"" ");
                                sb.Append(currentData);

                                if (matchFoundForRating && matchRating == k + 1)
                                {
                                    sb.Append(@" checked");
                                }

                                sb.Append(@">");
                                sb.Append(currentLabel);
                                sb.Append(@"</label>");
                            }

                            sb.Append(@"</fieldset><h5 class=""lv normal-custom"">Average Rating: ");
                            sb.Append(averageRating.ToString("F2", CultureInfo.InvariantCulture));
                            sb.Append(@"</h5></li>");
                        }
                        sb.Append(@"</ul></li>");
                    }
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }

        public string AddUpdateSongRating(string songrater, string songfilename, string rating)
        {
            try
            {
                using (var context = new UserSongModel(Utilities.GetConnectionStringForDatabase()))
                {
                    song songMatch = context.songs.Where(s => s.songfilename == songfilename).First();
                    List<Rating> ratingsList = Utilities.JsonDeSerialize<List<Rating>>(songMatch.ratings);
                    List<Rating> ratingsListMatch = ratingsList.Where(r => r.username == songrater).ToList();
                    if (ratingsListMatch.Count > 0)
                    {
                        ratingsList.Where(r => r.username == songrater).First().rating = int.Parse(rating);
                    }
                    else
                    {
                        ratingsList.Add(new Rating() { username = songrater, rating = int.Parse(rating) });
                    }
                    songMatch.ratings = Utilities.JsonSerialize(ratingsList);
                    context.SaveChanges();
                }

                return Utilities.SUCCESS;
            }
            catch (Exception)
            {
                return Utilities.EXCEPTION;
            }
        }
    }
}