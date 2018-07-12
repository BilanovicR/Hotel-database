using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sql1
{//C:\Users\User\Desktop\BP projekat Hotel\prof\sql1\sql1\Program.cs
    class Program
    {
        public static Random random = new Random();

        static void Main(string[] args)
        {
            //test();

            string connectionString = "Data Source=localhost;Initial Catalog=hoteldb;User ID=root;Password=";
            MySqlConnection conn = new MySqlConnection(connectionString);
            conn.Open();


            MySqlCommand command = conn.CreateCommand();

            createHotel(command);
            //fillHotel(command);
            selectHotel(command);

            conn.Close();
            Console.Write("Complete");
            Console.ReadKey();
        }

        private static void selectHotel(MySqlCommand command)
        {

            Console.WriteLine("1. Koji sektor ima najvise zalbi u prethodnih godinu dana?");

            command.CommandText = "SELECT department.department, COUNT(complaints.comment) AS topComplaints " +
                "FROM department, complaints WHERE complaints.department_id = department.id " +
                "AND complaints.date >= DATE_SUB(NOW(), INTERVAL 1 YEAR) GROUP BY department.id ORDER BY `topComplaints` DESC LIMIT 1";
            System.Data.DataSet ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Departman: " + ubb.Tables[0].Rows[podaci]["department"] + ", Broj zalbi: " + ubb.Tables[0].Rows[podaci]["topComplaints"]);
            }
            Console.WriteLine();

            Console.WriteLine("2. Koliki je ukupni profit ostvaren u prethodnoj godini?");

            command.CommandText = "SELECT (SELECT SUM(expenses.sum) FROM expenses WHERE expenses.date " +
                "BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) " +
                "AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR)) AS rashodi, " +
                "(SELECT SUM(bill_item.quantity * bill_item.item_price) FROM bill_item WHERE bill_item.date " +
                "BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), INTERVAL 1 YEAR) " +
                "AND DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-12-31'), INTERVAL 1 YEAR)) AS prihodi, (SELECT(rashodi - prihodi)) AS profit";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Rashodi: " + ubb.Tables[0].Rows[podaci]["rashodi"] + ", Prihodi: " + ubb.Tables[0].Rows[podaci]["prihodi"] + ", Profit: " + ubb.Tables[0].Rows[podaci]["profit"]);
            }
            Console.WriteLine();

            Console.WriteLine("3. Prikazi prosecnu godisnju i mesecnu popunjenost kapaciteta u prethodnih godinu dana.");

            command.CommandText = "SELECT (SELECT SUM(IF (reservation.departure <= NOW(), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)))*reservation.num_of_guests " +
                "FROM reservation WHERE reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) AS ukupanBrojNocenja, " +
                "(SELECT ukupanBrojNocenja / 3650 * 100) AS prosecnaGodisnjaPopunjenost, MONTHNAME(reservation.arrival) AS mesec, " +
                "SUM(DATEDIFF(reservation.departure, reservation.arrival))*reservation.num_of_guests / 300 * 100 AS mesecnaPopunjenost " +
                "FROM reservation WHERE(reservation.arrival BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND CURRENT_DATE()) " +
                "AND(reservation.departure BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND CURRENT_DATE()) " +
                "GROUP BY mesec order BY MONTH(reservation.arrival)";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Ukupan broj nocena: " + ubb.Tables[0].Rows[podaci]["ukupanBrojNocenja"] + ", Prosecna godisnja popunjenost: " + ubb.Tables[0].Rows[podaci]["prosecnaGodisnjaPopunjenost"] + ", Mesec: " + ubb.Tables[0].Rows[podaci]["mesec"] + ", Mesecna popunjenost: " + ubb.Tables[0].Rows[podaci]["mesecnaPopunjenost"]);
            }
            Console.WriteLine();

            Console.WriteLine("4. Koji su meseci u prethodnoj godini sa popunjenoscu kapaciteta iznad proseka?");

            command.CommandText = "SELECT svakiMesec.prosecnaMesecna, mesec FROM (SELECT MONTHNAME(reservation.arrival) AS 'mesec', " +
                "SUM(DATEDIFF(reservation.departure, reservation.arrival))*reservation.num_of_guests/300*100 AS 'prosecnaMesecna' " +
                "FROM reservation WHERE(reservation.arrival BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND CURRENT_DATE()) " +
                "AND(reservation.departure BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND CURRENT_DATE()) GROUP BY mesec ASC) svakiMesec, " +
                "(SELECT(SELECT SUM(IF(reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), " +
                "DATEDIFF(NOW(), reservation.arrival))) * reservation.num_of_guests FROM reservation WHERE reservation.arrival " +
                "BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND CURRENT_DATE()) AS 'ukupnaGodisnja', " +
                "(SELECT ukupnaGodisnja / 3650 * 100) AS 'prosecnaGodisnja') godisnja " +
                "WHERE svakiMesec.prosecnaMesecna > godisnja.prosecnaGodisnja order BY MONTH(mesec)";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Prosecna mesecna: " + ubb.Tables[0].Rows[podaci]["prosecnaMesecna"] + ", Mesec: " + ubb.Tables[0].Rows[podaci]["mesec"]);
            }
            Console.WriteLine();

            Console.WriteLine("5. Koliko su nocenja i dolazaka ostvarili domaci, a koliko strani gosti u poslednjih godinu dana po mesecima?");

            command.CommandText = "SELECT (SELECT SUM(IF(reservation.departure <= NOW(), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival))) * reservation.num_of_guests " +
                "FROM reservation, occupiedroom, guest WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id " +
                "AND guest.country = 'SRB') AS domaciNocenja, (SELECT SUM(IF(reservation.departure <= NOW(), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival))) * reservation.num_of_guests " +
                "FROM reservation, occupiedroom, guest WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id " +
                "AND guest.country != 'SRB') AS stranciNocenja, (SELECT SUM(IF(reservation.departure <= NOW(), 1, 0)) * reservation.num_of_guests " +
                "FROM reservation, occupiedroom, guest WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id AND guest.country = 'SRB') AS domaciDolasci, " +
                "(SELECT SUM(IF(reservation.departure <= NOW(), 1, 0)) * reservation.num_of_guests FROM reservation, occupiedroom, guest " +
                "WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) AND reservation.id = occupiedroom.reservation_id " +
                "AND occupiedroom.guest_id = guest.id AND guest.country != 'SRB') AS stranciDolasci";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Nocena domacih: " + ubb.Tables[0].Rows[podaci]["domaciNocenja"] + ", Nocena stranaca: " + ubb.Tables[0].Rows[podaci]["stranciNocenja"] + ", Dolasci domacih: " + ubb.Tables[0].Rows[podaci]["domaciDolasci"] + ", Dolasci stranaca: " + ubb.Tables[0].Rows[podaci]["stranciDolasci"]);
            }
            Console.WriteLine();

            Console.WriteLine("6. Koliko iznosi prosecna duzina boravka gostiju u hotelu?");

            command.CommandText = "SELECT SUM(DATEDIFF(reservation.departure, reservation.arrival))/COUNT(reservation.id) " +
                "AS prosecnaDuzinaBoravka FROM reservation";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Prosecna duzina boravka: " + ubb.Tables[0].Rows[podaci]["prosecnaDuzinaBoravka"]);
            }
            Console.WriteLine();

            Console.WriteLine("7. Prikazi sve agencije sortirane po visini ostvarenog prometa u rezervacijama u prethodnoj godini.");

            command.CommandText = "SELECT agency.name, COUNT(reservation.id) AS brojRezervacija, " +
                "sum(bill_item.quantity*bill_item.item_price) AS prihodi " +
                "FROM bill, bill_item, agency, agency_reservation, reservationtype, reservation, bill_reservation " +
                "WHERE bill_item.bill_id = bill.id AND bill_reservation.bill_id = bill.id AND bill_reservation.reservation_id = reservation.id " +
                "AND reservation.reservationType_id = reservationtype.id AND reservationtype.type = 'Agency' " +
                "AND agency_reservation.reservation_id = reservation.id AND agency_reservation.agency_id = agency.id " +
                "AND reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), INTERVAL 1 YEAR) " +
                "AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR) GROUP BY agency.name ORDER BY `prihodi` DESC";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Naziv: " + ubb.Tables[0].Rows[podaci]["name"] + ", Broj rezervacija: " + ubb.Tables[0].Rows[podaci]["brojRezervacija"] + ", Prihodi: " + ubb.Tables[0].Rows[podaci]["prihodi"]);
            }
            Console.WriteLine();

            Console.WriteLine("8. Prikazi procentualnu zastupljenost nacina placanja u poslednjih godinu dana.");

            command.CommandText = "SELECT payment.method, (count(payment.method)/(SELECT count(bill.id) FROM bill, bill_item " +
                "WHERE bill_item.bill_id = bill.id AND bill_item.date >= DATE_SUB(NOW(),INTERVAL 1 YEAR))*100) AS procenat " +
                "FROM bill_item, bill, payment WHERE bill_item.bill_id = bill.id AND bill.payment_id = payment.id " +
                "AND bill_item.date >= DATE_SUB(NOW(),INTERVAL 1 YEAR) GROUP BY payment.method";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Nacin placanja: " + ubb.Tables[0].Rows[podaci]["method"] + ", Procenat: " + ubb.Tables[0].Rows[podaci]["procenat"]);
            }
            Console.WriteLine();

            Console.WriteLine("9. Prikazi sve agencije koje imaju neplacene racune za ostvarene rezervacije i njihov iznos.");

            command.CommandText = "SELECT agency.name, SUM(bill_item.item_price*bill_item.quantity) AS suma " +
                "FROM bill_item, bill, payment, bill_reservation, reservation, reservationtype, agency_reservation, agency, reservationStatus " +
                "WHERE agency.id = agency_reservation.agency_id AND agency_reservation.reservation_id = reservation.id " +
                "AND reservation.reservationType_id = 2 AND reservation.id = bill_reservation.reservation_id " +
                "AND bill_reservation.bill_id = bill.id AND bill_item.bill_id = bill.id AND payment.id = bill.payment_id " +
                "AND payment.method = 'Neplaceno' AND reservation.reservationStatus_id = reservationStatus.id " +
                "AND reservationStatus.type = 'Potvrdjena' GROUP BY agency.name";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Naziv: " + ubb.Tables[0].Rows[podaci]["name"] + ", Suma: " + ubb.Tables[0].Rows[podaci]["suma"]);
            }
            Console.WriteLine();

            Console.WriteLine("10. Koliki je broj i procenat otkazanih rezervacija u poslednjih godinu?");

            command.CommandText = "SELECT ((SELECT COUNT(reservation.id) AS brojOtkazanih FROM reservation, reservationstatus " +
                "WHERE reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Otkazana' " +
                "AND reservation.arrival BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND NOW())/ (SELECT COUNT(reservation.id) AS brojUkupnih " +
                "FROM reservation, reservationstatus WHERE reservation.reservationStatus_id = reservationstatus.id AND reservation.arrival " +
                "BETWEEN DATE_SUB(NOW(), INTERVAL 1 YEAR) AND NOW()))*100 AS procenat";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Procenat: " + ubb.Tables[0].Rows[podaci]["procenat"]);
            }
            Console.WriteLine();

            Console.WriteLine("11. Koliki je iznos i procenat naplacenih otkazanih rezervacija preko booking.com u poslednjih godinu dana?");

            command.CommandText = "SELECT total.totalRez AS ukupnoOstvarenoRezervacija, " +
                "otkazane.brrez/total.totalRez*100 AS procenatOtkazanihRezervacija, otkazane.prihodi AS prihodiOdOtkazanih " +
                "FROM (SELECT count(reservation.id) AS brRez, sum(bill_item.quantity * bill_item.item_price) AS prihodi " +
                "FROM reservation, reservationtype, bill_reservation, agency_reservation, agency, reservationstatus, bill, payment, bill_item " +
                "WHERE bill_item.bill_id = bill.id AND bill_reservation.bill_id = bill.id AND bill.payment_id = payment.id " +
                "AND payment.method != 'Neplaceno' AND bill_reservation.reservation_id = reservation.id " +
                "AND agency_reservation.reservation_id = reservation.id AND reservation.reservationStatus_id = reservationstatus.id " +
                "AND reservationstatus.type = 'Otkazana' AND reservation.reservationType_id = reservationtype.id " +
                "AND reservationtype.type = 'Agency' AND agency_reservation.agency_id = agency.id AND agency.name = 'Booking' " +
                "AND reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR)) otkazane, " +
                "(SELECT COUNT(reservation.id) AS totalRez FROM reservation, reservationstatus " +
                "WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id " +
                "AND reservationstatus.type = 'Potvrdjena') total";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Ostvareno: " + ubb.Tables[0].Rows[podaci]["ukupnoOstvarenoRezervacija"] + ", Procenat: " + ubb.Tables[0].Rows[podaci]["procenatOtkazanihRezervacija"] + ", Prihodi: " + ubb.Tables[0].Rows[podaci]["prihodiOdOtkazanih"]);
            }
            Console.WriteLine();

            Console.WriteLine("12. Navedi sve drzave iz kojih su dosli gosti sortirano po broju dolazaka.");

            command.CommandText = "SELECT count(occupiedroom.reservation_id) AS dolasci, guest.country " +
                "FROM reservation, occupiedroom, guest WHERE reservation.id = occupiedroom.reservation_id " +
                "AND occupiedroom.guest_id = guest.id GROUP BY guest.country ORDER BY dolasci DESC";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Dolasci: " + ubb.Tables[0].Rows[podaci]["dolasci"] + ", Drzava: " + ubb.Tables[0].Rows[podaci]["country"]);
            }
            Console.WriteLine();

            Console.WriteLine("13. Koji je procenat ostvarenih rezervacija koje dolaze preko web sajta u poslednjih godinu dana po mesecima?");

            command.CommandText = "SELECT MONTH(reservation.arrival) AS mesec, (count(reservation.id)/(SELECT COUNT(reservation.id) " +
                "FROM reservation WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR))*100) AS procenat " +
                "FROM reservation, reservationtype WHERE reservation.reservationType_id = reservationtype.id " +
                "AND reservationtype.type = 'Web site' AND reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) order BY mesec";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Mesec: " + ubb.Tables[0].Rows[podaci]["mesec"] + ", Procenat: " + ubb.Tables[0].Rows[podaci]["procenat"]);
            }
            Console.WriteLine();

            Console.WriteLine("14. Koji je broj i ukupna vrednost naplacenih boravisnih taksi u prethodnom mesecu?");

            command.CommandText = "SELECT SUM(bill_reservation.residence_tax_id_tax * DATEDIFF(reservation.departure, reservation.arrival)) " +
                "AS brojTaksi, (SUM(bill_reservation.residence_tax_id_tax * DATEDIFF(reservation.departure, reservation.arrival)) * residence_tax.tax_total) " +
                "AS ukupnaVrednost FROM residence_tax, bill_reservation, bill, bill_item, reservation " +
                "WHERE residence_tax.id_tax = bill_reservation.residence_tax_id_tax AND bill.id = bill_reservation.bill_id " +
                "AND bill.id = bill_item.bill_id AND bill_item.date " +
                "BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-%m-01'), INTERVAL 1 MONTH) AND LAST_DAY(NOW() -INTERVAL 1 MONTH)";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Broj taksi: " + ubb.Tables[0].Rows[podaci]["brojTaksi"] + ", Ukupna vrednost: " + ubb.Tables[0].Rows[podaci]["ukupnaVrednost"]);
            }
            Console.WriteLine();

            Console.WriteLine("15. Prikazi dnevni izvestaj (sva placanja u toku danasnjeg dana sortirana po nacinu placanja).");

            command.CommandText = "SELECT payment.method, SUM(bill_item.item_price*bill_item.quantity) AS prihod FROM bill_item, bill, payment " +
                "WHERE bill_item.bill_id = bill.id AND bill.payment_id = payment.id AND bill_item.date = CURDATE() GROUP BY payment.method";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Nacin placana: " + ubb.Tables[0].Rows[podaci]["method"] + ", Prihod: " + ubb.Tables[0].Rows[podaci]["prihod"]);
            }
            Console.WriteLine();

            Console.WriteLine("16. Koliko je ostvareno nocenja u prethodnom mesecu u odnosu na isti mesec prosle godine?");

            command.CommandText = "SELECT oveGodine.brojNocenja AS 'oveGodine', prosleGodine.brNocenja AS 'prosleGodine', " +
                "oveGodine.brojNocenja-prosleGodine.brNocenja AS 'razlika' " +
                "FROM (SELECT(SUM(IF(reservation.departure <= LAST_DAY(NOW() - INTERVAL 1 MONTH), " +
                "DATEDIFF(reservation.departure, reservation.arrival), " +
                "DATEDIFF(LAST_DAY(NOW() - INTERVAL 1 MONTH), reservation.arrival)) * reservation.num_of_guests)) AS 'brojNocenja' " +
                "FROM reservation WHERE reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-%m-01'), INTERVAL 1 MONTH) " +
                "AND LAST_DAY(NOW() - INTERVAL 1 MONTH)) oveGodine, " +
                "(SELECT(SUM(IF(reservation.departure <= DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR), " +
                "reservation.arrival))) * reservation.num_of_guests) AS 'brNocenja' FROM reservation WHERE(reservation.arrival " +
                "BETWEEN DATE_SUB(DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-%m-01'), INTERVAL 1 MONTH), INTERVAL 1 YEAR) " +
                "AND DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR))) prosleGodine";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Ove godine: " + ubb.Tables[0].Rows[podaci]["oveGodine"] + ", Prosle godine: " + ubb.Tables[0].Rows[podaci]["prosleGodine"] + ", Razlika: " + ubb.Tables[0].Rows[podaci]["razlika"]);
            }
            Console.WriteLine();

            Console.WriteLine("17. Koliki je procenat gostiju koji su ostvarili vise od 10 nocenja u prethodnoj godini?");

            command.CommandText = "SELECT total.ukupno as 'ukupnoNocenjaProsleGodine', prosleGodine.viseOdDeset " +
                "AS 'BrojGostijuSaViseOdDesetNocenja', prosleGodine.viseOdDeset/total.ukupno AS procenat " +
                "FROM (SELECT COUNT(brojNocenjaPoGostu.brojNocenja) AS 'viseOdDeset' FROM(SELECT SUM(IF(reservation.departure <= NOW(), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)) * reservation.num_of_guests) " +
                "AS brojNocenja FROM reservation, reservationstatus, occupiedroom, guest WHERE reservation.arrival " +
                "BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), INTERVAL 1 YEAR) " +
                "AND DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-12-31'), INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id " +
                "AND reservationstatus.type = 'Potvrdjena' AND occupiedroom.reservation_id = reservation.id " +
                "AND occupiedroom.guest_id = guest.id GROUP BY guest.id) brojNocenjaPoGostu " +
                "WHERE brojNocenjaPoGostu.brojNocenja > 10) prosleGodine, " +
                "(SELECT(SUM(IF(reservation.departure <= DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-12-31'), INTERVAL 1 YEAR), " +
                "DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-12-31'), INTERVAL 1 YEAR), " +
                "reservation.arrival))) * reservation.num_of_guests) AS 'ukupno' FROM reservation, reservationstatus WHERE reservation.arrival " +
                "BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'), " +
                "INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena') total";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Ukupno nocenja prosle godine: " + ubb.Tables[0].Rows[podaci]["ukupnoNocenjaProsleGodine"] + ", Broj gostiju sa vise od deset nocenja: " + ubb.Tables[0].Rows[podaci]["BrojGostijuSaViseOdDesetNocenja"] + ", Procenat: " + ubb.Tables[0].Rows[podaci]["procenat"]);
            }
            Console.WriteLine();

            Console.WriteLine("18. Koliki je procenat ostvarenih rezervacija u pusackim sobama po mesecima u poslednih godinu dana?");

            command.CommandText = "SELECT MONTH(reservation.arrival) AS mesec, (count(reservation.id) / (SELECT COUNT(reservation.id) " +
                "FROM reservation, reservationstatus WHERE reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) " +
                "AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena')*100) AS procenat " +
                "FROM reservation, reservationstatus, occupiedroom, room WHERE reservation.reservationStatus_id = reservationstatus.id " +
                "AND reservationstatus.type = 'Potvrdjena' AND reservation.id = occupiedroom.reservation_id " +
                "AND occupiedroom.room_id = room.id AND room.smoking = 'Y' " +
                "AND reservation.arrival >= DATE_SUB(NOW(), INTERVAL 1 YEAR) ORDER BY mesec";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Mesec: " + ubb.Tables[0].Rows[podaci]["mesec"] + ", Procenat: " + ubb.Tables[0].Rows[podaci]["procenat"]);
            }
            Console.WriteLine();

            Console.WriteLine("19. Koliko dana je konferencijska sala bila popunjena u toku prvog tromesecja tekuce i prethodne godine?");

            command.CommandText = "SELECT tekuceTromesecje.brojDana as 'TromesecjeOveGodine', " +
                "prosloTromesecje.brojDana AS 'TromesecjeProsleGodine' FROM (SELECT SUM(bill_item.quantity) AS brojDana " +
                "FROM services, bill_item WHERE services.type = 'CONFERENCE ROOM' AND services.id = bill_item.services_id_service " +
                "AND bill_item.date BETWEEN DATE_FORMAT(CURRENT_DATE, '%Y-01-01') AND DATE_FORMAT(CURRENT_DATE, '%Y-03-31')) tekuceTromesecje, " +
                "(SELECT SUM(bill_item.quantity) AS brojDana FROM services, bill_item WHERE services.type = 'CONFERENCE ROOM' " +
                "AND services.id = bill_item.services_id_service AND bill_item.date BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), " +
                "INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-03-31'),INTERVAL 1 YEAR)) prosloTromesecje";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Tromesecje ove godine: " + ubb.Tables[0].Rows[podaci]["TromesecjeOveGodine"] + ", Tromesecje prosle godine: " + ubb.Tables[0].Rows[podaci]["TromesecjeProsleGodine"]);
            }
            Console.WriteLine();

            Console.WriteLine("20. Iz kojih delatnosti su firme sa kojima je ostvareno najvise nocenja u prethodnoj godini?");

            command.CommandText = "SELECT agency.industry, DATEDIFF(reservation.departure, reservation.arrival) * reservation.num_of_guests " +
                "AS brojNocenja FROM agency, agency_reservation, reservationtype, reservation, reservationstatus " +
                "WHERE reservation.reservationType_id = 2 AND reservation.reservationStatus_id = 1 " +
                "AND agency_reservation.reservation_id = reservation.id AND agency_reservation.agency_id = agency.id " +
                "AND reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE, '%Y-01-01'), INTERVAL 1 YEAR) " +
                "AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR) GROUP BY agency.industry ORDER BY brojNocenja DESC";
            ubb = executeSelect(command);

            for (int podaci = 0; podaci < ubb.Tables[0].Rows.Count; ++podaci)
            {
                Console.WriteLine("Delatnost: " + ubb.Tables[0].Rows[podaci]["industry"] + ", Broj nocena: " + ubb.Tables[0].Rows[podaci]["brojNocenja"]);
            }
            Console.WriteLine();


        }

        private static void fillHotel(MySqlCommand command)
        {

            command.CommandText = "INSERT INTO `hoteldb`.`room_type` (`id`, `type`) VALUES (1, 'DBL'),"
                + "(2, 'TWIN'),"
                + "(3, '2DBL'),"
                + "(4, 'SUITE');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`room` (`id`, `smoking`, `room_type_id`) VALUES (1, 'Y', 1),"
                + "(2, 'N', 1),"
                + "(3, 'Y', 2),"
                + "(4, 'N', 2),"
                + "(5, 'Y', 3),"
                + "(6, 'N', 3),"
                + "(7, 'Y', 1),"
                + "(8, 'N', 2),"
                + "(9, 'Y', 3),"
                + "(10, 'Y', 4);";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`reservationStatus` (`id`, `type`) VALUES (1, 'Potvrdjena'),"
                + "(2, 'Otkazana'),"
                + "(3, 'No-show');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`reservationType` (`id`, `type`) VALUES (1, 'Individualna'),"
                + "(2, 'Agencija'),"
                + "(3, 'Web site'),"
                + "(4, 'Walk-in');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`payment` (`id`, `method`) VALUES (1, 'Gotovina'),"
                + "(2, 'KK'),"
                + "(3, 'Virman'),"
                + "(4, 'Neplaceno');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`hotel` (`idhotel`, `hotel_name`) VALUES (1, 'Ruzmark');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`department` (`id`, `department`, `hotel_idhotel`) VALUES (1, 'Front Office', 1),"
                + "(2, 'Housekeeping', 1),"
                + "(3, 'Restaurant', 1),"
                + "(4, 'Event Management', 1),"
                + "(5, 'Management', 1);";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`services` (`id`, `type`, `price`, `department_id`) VALUES (1, 'ROOM CHARGE DBL', 75, 2),"
                + "(2, 'ROOM CHARGE TWIN', 100, 2),"
                + "(3, 'ROOM CHARGE 2DBL', 150, 2),"
                + "(4, 'ROOM CHARGE SUITE', 200, 2),"
                + "(5, 'RESTAURANT 1', 20, 3),"
                + "(6, 'MINI-BAR', 5, 2),"
                + "(7, 'LAUNDRY 1', 2.50, 2),"
                + "(8, 'LAUNDRY 2', 3, 2),"
                + "(9, 'RESTAURANT 2', 15, 3),"
                + "(10, 'CONFERENCE ROOM', 1000, 4);";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`typeOfExpense` (`id`, `type`) VALUES (1, 'Elektricna energija'),"
                + "(2, 'Komunalije'),"
                + "(3, 'Internet'),"
                + "(4, 'Telefon'),"
                + "(5, 'Odrzavanje web sajta'),"
                + "(6, 'Obezbedjenje'),"
                + "(7, 'Nabavka - restoran'),"
                + "(8, 'Nabavka - recepcija'),"
                + "(9, 'Nabavka - domacinstvo'),"
                + "(10, 'Zarade'),"
                + "(11, 'Marketing'),"
                + "(12, 'Dekoracija'),"
                + "(13, 'Uniforme'),"
                + "(14, 'Dnevna Stampa');";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`residence_tax` (`id_tax`, `tax_total`) VALUES (1, 1.5),"
                + "(2, 3),"
                + "(3, 4.5),"
                + "(4, 6);";
            executeNonQuery(command);

            command.CommandText = "INSERT INTO `hoteldb`.`agency` (`id`, `name`, `discount`, `industry`) VALUES (1, 'Booking', 10, 'Web reservations'),"
                + "(2, 'NS Travel', 10, 'Tourism'),"
                + "(3, 'NS Banking', 15, 'Finance'),"
                + "(4, 'NS IT', 10, 'IT'),"
                + "(5, 'NS Hospital', 20, 'Healthcare'),"
                + "(6, 'NS Agro', 15, 'Agriculture'),"
                + "(7, 'NS University', 15, 'Education'),"
                + "(8, 'NS Tourism', 10, 'Tourism'),"
                + "(9, 'Serbia Banking', 20, 'Finance'),"
                + "(10, 'Serbia Petroleum', 20, 'Petroleum'),"
                + "(11, 'Serbia Health Institute', 20, 'Healthcare'),"
                + "(12, 'FSS', 10, 'Sport'),"
                + "(13, 'FKV', 15, 'Sport'),"
                + "(14, 'VSS', 10, 'Sport'),"
                + "(15, 'RSS', 15, 'Sport'),"
                + "(16, 'LKS', 15, 'Healthcare'),"
                + "(17, 'NIS', 20, 'Petroleum'),"
                + "(18, 'EBank', 20, 'Finance'),"
                + "(19, 'Serbia IT', 20, 'IT'),"
                + "(20, 'TUI', 15, 'Tourism'),"
                + "(21, 'TONS', 15, 'Tourism'),"
                + "(22, 'TOV', 15, 'Tourism'),"
                + "(23, 'CodeIT', 20, 'IT'),"
                + "(24, 'NSMedia', 10, 'Media'),"
                + "(25, 'NSBooks', 10, 'Publishing');";
            executeNonQuery(command);

            List<string> guestName = new List<string>();
            guestName.Add("Marko");
            guestName.Add("Stefan");
            guestName.Add("Nikola");
            guestName.Add("Ivan");
            guestName.Add("Petar");
            guestName.Add("Srdjan");
            guestName.Add("Igor");
            guestName.Add("Lazar");
            guestName.Add("Nebojsa");
            guestName.Add("Bojan");
            guestName.Add("Jovan");
            guestName.Add("Ruzica");
            guestName.Add("Ivana");
            guestName.Add("Milica");
            guestName.Add("Marija");
            guestName.Add("Marina");

            List<string> guestLastName = new List<string>();
            guestLastName.Add("Stefanov");
            guestLastName.Add("Markovic");
            guestLastName.Add("Nikolic");
            guestLastName.Add("Petrovic");
            guestLastName.Add("Ivanov");
            guestLastName.Add("Ruzic");
            guestLastName.Add("Milic");
            guestLastName.Add("Maric");
            guestLastName.Add("Bojanic");
            guestLastName.Add("Ilic");
            guestLastName.Add("Predic");
            guestLastName.Add("Jovanovic");
            guestLastName.Add("Jovic");

            List<string> guestCountry = new List<string>();
            guestCountry.Add("SRB");
            guestCountry.Add("CRO");
            guestCountry.Add("BiH");
            guestCountry.Add("MK");
            guestCountry.Add("MNE");

            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndName = rnd.Next(0, guestName.Count);
                int rndLastName = rnd.Next(0, guestLastName.Count);
                int rndCountry = rnd.Next(0, guestCountry.Count);
                command.CommandText = "INSERT INTO `hoteldb`.`guest` (`first_name`, `last_name`, `country`, `documentID`) VALUES "
                    + "('" + guestName[rndName]
                    + "', '" + guestLastName[rndLastName]
                    + "', '" + guestCountry[rndCountry]
                    + "', '" + rnd.Next(100000, 999999)
                    + "');";
                executeNonQuery(command);
            };


            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();

                DateTime rndDate = RandomDay();
                int rndSum = rnd.Next(10, 5000);
                int rndtype = rnd.Next(1, 15);
                command.CommandText = "INSERT INTO `hoteldb`.`expenses` (`date`, `sum`, `typeOfExpense_id`, `hotel_idhotel`) VALUES "
                    + "('" + dtfordb(rndDate)
                    + "', '" + rndSum
                    + "', '" + rndtype
                    + "', '" + 1
                    + "');";
                executeNonQuery(command);
            }

            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndDep = rnd.Next(1, 6);//ima 5 departmana/sektora
                int rndLength = rnd.Next(10, 30);//duzina stringa varira
                DateTime rndDate = RandomDay();
                command.CommandText = "INSERT INTO `hoteldb`.`complaints` (`comment`, `department_id`, `date`) VALUES "
                    + "('" + randStr(rndLength)
                    + "', '" + rndDep
                    + "', '" + dtfordb(rndDate)
                    + "');";
                executeNonQuery(command);
            }



            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndPayment = rnd.Next(1, 5);//ima 4 nacina placanja
                command.CommandText = "INSERT INTO `hoteldb`.`bill` (`payment_id`) VALUES "
                    + "('" + rndPayment
                    + "');";
                executeNonQuery(command);
            };


            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndStatus = rnd.Next(1, 4);//ima 3 statusa
                int rndType = rnd.Next(1, 5);//ima 4 nacina placanja
                int rndGuests = rnd.Next(1, 5);//max 4 gosta
                DateTime rndArr = RandomDay();
                DateTime rndDep = rndArr.AddDays(rnd.Next(1, 10));
                command.CommandText = "INSERT INTO `hoteldb`.`reservation` (`arrival`, `departure`, `reservationStatus_id`, `reservationType_id`, `num_of_guests`) VALUES "
                    + "('" + dtfordb(rndArr)
                    + "', '" + dtfordb(rndDep)
                    + "', '" + rndStatus
                    + "', '" + rndType
                    + "', '" + rndGuests
                    + "');";
                executeNonQuery(command);
            };

            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndRoom = rnd.Next(1, 11);//ima 10 soba
                int rndRes = rnd.Next(1, 21);//ima 20 rezervacija
                int rndGuest = rnd.Next(1, 21);//ima 20 gostiju
                command.CommandText = "INSERT INTO `hoteldb`.`occupiedRoom` (`room_id`, `reservation_id`, `guest_id`) VALUES "
                    + "('" + rndRoom
                    + "', '" + rndRes
                    + "', '" + rndGuest
                    + "');";
                executeNonQuery(command);
            };


            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndBill = rnd.Next(1, 21);//ima 20 racuna
                int rndRes = rnd.Next(1, 21);//ima 20 rezervacija
                int rndTax = rnd.Next(1, 5);//ima max 4 takse
                command.CommandText = "INSERT INTO `hoteldb`.`bill_reservation` (`reservation_id`, `bill_id`, `residence_tax_id_tax`) VALUES "
                    + "('" + rndRes
                    + "', '" + rndBill
                    + "', '" + rndTax
                    + "');";
                executeNonQuery(command);
            };

            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndQty = rnd.Next(1, 5);//kolicina
                int rndPrice = rnd.Next(1, 1000);//random cena da se ne bi vrsio upit ponovni, i da bi se pamtilo u slucaju promene cena 
                int rndBill = rnd.Next(1, 21);//ima 20 racuna
                int rndService = rnd.Next(1, 11);//ima 10 usluga u cenovniku
                DateTime rndDate = RandomDay();
                command.CommandText = "INSERT INTO `hoteldb`.`bill_item` (`quantity`, `item_price`, `services_id_service`, `bill_id`, `date`) VALUES "
                    + "('" + rndQty
                    + "', '" + rndPrice
                    + "', '" + rndService
                    + "', '" + rndBill
                    + "', '" + dtfordb(rndDate)
                    + "');";
                executeNonQuery(command);
            };
            for (int i = 1; i <= 20; i++)
            {
                Random rnd = new Random();
                int rndAgency = rnd.Next(1, 26);//ima 25 agencija
                int rndRes = rnd.Next(1, 21);//ima 20 rezervacija
                command.CommandText = "INSERT INTO `hoteldb`.`agency_reservation` (`reservation_id`, `agency_id`) VALUES "
                    + "('" + rndRes
                    + "', '" + rndAgency
                    + "');";
                executeNonQuery(command);


            };
        }

        private static void createHotel(MySqlCommand command)
        {
            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`room_type` ("
                            + "`id` INT NOT NULL, "
                            + "`type` VARCHAR(45) NOT NULL, "
                            + "PRIMARY KEY (`id`))";
            executeNonQuery(command); //command.ExecuteNonQuery();

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`room` ("
                            + "`id` INT NOT NULL, "
                            + "`smoking` VARCHAR(45) NOT NULL, "
                            + "`room_type_id` INT NOT NULL, "
                            + "PRIMARY KEY (`id`), "
                            + "INDEX `fk_room_room_type1_idx` (`room_type_id` ASC), "
                            + "CONSTRAINT `fk_room_room_type1` "
                            + "FOREIGN KEY (`room_type_id`) "
                            + "REFERENCES `hoteldb`.`room_type` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`guest` ("
                            + "`id` INT NOT NULL AUTO_INCREMENT, "
                            + "`first_name` VARCHAR(45) NOT NULL, "
                            + "`last_name` VARCHAR(45) NOT NULL, "
                            + "`country` VARCHAR(45) NOT NULL, "
                            + "`documentID` VARCHAR(45) NOT NULL, "
                            + "PRIMARY KEY (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`reservationStatus` ("
                             + "`id` INT NOT NULL AUTO_INCREMENT, "
                             + "`type` VARCHAR(45) NOT NULL, "
                             + "PRIMARY KEY (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`agency` ("
                            + "`id` INT NOT NULL AUTO_INCREMENT, "
                            + "`name` VARCHAR(45) NOT NULL, "
                            + "`discount` INT NOT NULL, "
                            + "`industry` VARCHAR(45) NOT NULL, "
                            + "PRIMARY KEY (`id`))";
            executeNonQuery(command);


            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`reservationType` ("
                            + "`id` INT NOT NULL AUTO_INCREMENT, "
                            + "`type` VARCHAR(45) NOT NULL, "
                            //+ "`agency_id` INT NULL, "
                            + "PRIMARY KEY (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`reservation` ("
                            + "`id` INT NOT NULL AUTO_INCREMENT, "
                            + "`arrival` DATE NOT NULL, "
                            + "`departure` DATE NOT NULL, "
                            + "`reservationStatus_id` INT NOT NULL, "
                            + "`reservationType_id` INT NOT NULL, "
                            + "`num_of_guests` INT NOT NULL, "
                            + "PRIMARY KEY (`id`), "
                            + "INDEX `fk_reservation_reservationStatus1_idx` (`reservationStatus_id` ASC), "
                            + "INDEX `fk_reservation_reservationType1_idx` (`reservationType_id` ASC), "
                            + "CONSTRAINT `fk_reservation_reservationStatus1` "
                            + "FOREIGN KEY (`reservationStatus_id`) "
                            + "REFERENCES `hoteldb`.`reservationStatus` (`id`), "
                            + "CONSTRAINT `fk_reservation_reservationType1` "
                            + "FOREIGN KEY (`reservationType_id`) "
                            + "REFERENCES `hoteldb`.`reservationType` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`payment` ("
                            + "`id` INT NOT NULL AUTO_INCREMENT, "
                            + "`method` VARCHAR(45) NOT NULL, "
                            + "PRIMARY KEY (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`bill` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`payment_id` INT NOT NULL, "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_bill_payment1_idx` (`payment_id` ASC), "
                         + "CONSTRAINT `fk_bill_payment1` "
                         + "FOREIGN KEY (`payment_id`) "
                         + "REFERENCES `hoteldb`.`payment` (`id`)) ";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`hotel` ("
                         + "`idhotel` INT NOT NULL, "
                         + "`hotel_name` VARCHAR(45) NOT NULL, "
                         + "PRIMARY KEY (`idhotel`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`department` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`department` VARCHAR(45) NOT NULL, "
                         + "`hotel_idhotel` INT NOT NULL, "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_department_hotel1_idx` (`hotel_idhotel` ASC), "
                         + "CONSTRAINT `fk_department_hotel1` "
                         + "FOREIGN KEY (`hotel_idhotel`) "
                         + "REFERENCES `hoteldb`.`hotel` (`idhotel`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`services` ("
                         + "`id` INT NOT NULL , "
                         + "`type` VARCHAR(45) NOT NULL, "
                         + "`price` DOUBLE NOT NULL, "
                         + "`department_id` INT NOT NULL, "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_services_department1_idx` (`department_id` ASC), "
                         + "CONSTRAINT `fk_services_department1` "
                         + "FOREIGN KEY (`department_id`) "
                         + "REFERENCES `hoteldb`.`department` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`bill_item` ("
                         + "`quantity` INT NOT NULL, "
                         + "`item_price` INT NOT NULL, "
                         + "`services_id_service` INT NOT NULL, "
                         + "`bill_id` INT NOT NULL, "
                         + "`date` DATE NOT NULL, "
                         + "INDEX `fk_bill_item_bill1_idx` (`bill_id` ASC), "
                         + "CONSTRAINT `fk_bill_item_services1` "
                         + "FOREIGN KEY (`services_id_service`) "
                         + "REFERENCES `hoteldb`.`services` (`id`), "
                         + "CONSTRAINT `fk_bill_item_bill1` "
                         + "FOREIGN KEY (`bill_id`) "
                         + "REFERENCES `hoteldb`.`bill` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`typeOfExpense` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`type` VARCHAR(45) NOT NULL, "
                         + "PRIMARY KEY (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`expenses` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`date` DATE NOT NULL, "
                         + "`sum` VARCHAR(45) NOT NULL, "
                         + "`typeOfExpense_id` INT NOT NULL, "
                         + "`hotel_idhotel` INT NOT NULL, "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_expenses_typeOfExpense1_idx` (`typeOfExpense_id` ASC), "
                         + "INDEX `fk_expenses_hotel1_idx` (`hotel_idhotel` ASC), "
                         + "CONSTRAINT `fk_expenses_typeOfExpense1` "
                         + "FOREIGN KEY (`typeOfExpense_id`) "
                         + "REFERENCES `hoteldb`.`typeOfExpense` (`id`), "
                         + "CONSTRAINT `fk_expenses_hotel1` "
                         + "FOREIGN KEY (`hotel_idhotel`) "
                         + "REFERENCES `hoteldb`.`hotel` (`idhotel`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`complaints` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`comment` VARCHAR(255) NOT NULL, "
                         + "`department_id` INT NOT NULL, "
                         + "`date` DATE NOT NULL, "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_complaints_department1_idx` (`department_id` ASC), "
                         + "CONSTRAINT `fk_complaints_department1` "
                         + "FOREIGN KEY (`department_id`) "
                         + "REFERENCES `hoteldb`.`department` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`occupiedRoom` ("
                         + "`id` INT NOT NULL AUTO_INCREMENT, "
                         + "`room_id` INT NOT NULL, "
                         + "`reservation_id` INT NOT NULL, "
                         + "`guest_id` INT NOT NULL, "
                         + "INDEX `fk_occupiedRoom_reservation1_idx` (`reservation_id` ASC), "
                         + "PRIMARY KEY (`id`), "
                         + "INDEX `fk_occupiedRoom_guest1_idx` (`guest_id` ASC), "
                         + "CONSTRAINT `fk_occupiedRoom_room1` "
                         + "FOREIGN KEY (`room_id`) "
                         + "REFERENCES `hoteldb`.`room` (`id`), "
                         + "CONSTRAINT `fk_occupiedRoom_reservation1` "
                         + "FOREIGN KEY (`reservation_id`) "
                         + "REFERENCES `hoteldb`.`reservation` (`id`), "
                         + "CONSTRAINT `fk_occupiedRoom_guest1` "
                         + "FOREIGN KEY (`guest_id`) "
                         + "REFERENCES `hoteldb`.`guest` (`id`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`residence_tax` ("
                         + "`id_tax` INT NOT NULL AUTO_INCREMENT, "
                         + "`tax_total` DOUBLE NOT NULL, "
                         + "PRIMARY KEY (`id_tax`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`bill_reservation` ("
                         + "`reservation_id` INT NOT NULL, "
                         + "`bill_id` INT NOT NULL, "
                         + "`residence_tax_id_tax` INT NOT NULL, "
                         + "INDEX `fk_bill_reservation_reservation1_idx` (`reservation_id` ASC), "
                         + "INDEX `fk_bill_reservation_bill1_idx` (`bill_id` ASC), "
                         + "INDEX `fk_bill_reservation_residence_tax1_idx` (`residence_tax_id_tax` ASC), "
                         + "CONSTRAINT `fk_bill_reservation_reservation1` "
                         + "FOREIGN KEY (`reservation_id`) "
                         + "REFERENCES `hoteldb`.`reservation` (`id`), "
                         + "CONSTRAINT `fk_bill_reservation_bill1` "
                         + "FOREIGN KEY (`bill_id`) "
                         + "REFERENCES `hoteldb`.`bill` (`id`), "
                         + "CONSTRAINT `fk_bill_reservation_residence_tax1` "
                         + "FOREIGN KEY (`residence_tax_id_tax`) "
                         + "REFERENCES `hoteldb`.`residence_tax` (`id_tax`))";
            executeNonQuery(command);

            command.CommandText = "CREATE TABLE IF NOT EXISTS `hoteldb`.`agency_reservation` ("
                         + "`agency_id` INT NOT NULL, "
                         + "`reservation_id` INT NOT NULL, "
                         + "INDEX `fk_agency_reservation_reservation1_idx` (`reservation_id` ASC), "
                         + " INDEX `fk_agency_reservation_agency1_idx` (`agency_id` ASC), "
                         + "CONSTRAINT `fk_agency_reservation_reservation1` "
                         + "FOREIGN KEY (`reservation_id`) "
                         + "REFERENCES `hoteldb`.`reservation` (`id`), "
                         + "CONSTRAINT `fk_agency_reservation_agency1` "
                         + " FOREIGN KEY (`agency_id`) "
                         + "REFERENCES `hoteldb`.`agency` (`id`));";
            executeNonQuery(command);
        }

        public static string dtfordb(DateTime dt)
        {
            return dt.Year + "-" + dt.Month + "-" + dt.Day + " " + dt.TimeOfDay;
        }


        public static void executeNonQuery(MySqlCommand commandsw)
        {
            bool b = false;

            while (!b)
            {
                try
                {
                    if (!commandsw.Connection.Ping() || commandsw.Connection.State == System.Data.ConnectionState.Closed)
                    {
                        commandsw.Connection.Open();
                    }
                    commandsw.ExecuteNonQuery();
                    b = true;
                    Console.WriteLine("napravljena tabela");
                }
                catch
                {
                    Console.WriteLine(DateTime.Now + "Vrtimo se u petlji upisa u bazu");
                    Console.WriteLine(DateTime.Now + " " + commandsw.CommandText);
                    b = false;
                }
            }
        }

        public static System.Data.DataSet executeSelect(MySqlCommand command)
        {
            bool b = false;
            System.Data.DataSet cas = new System.Data.DataSet();

            while (!b)
            {
                try
                {
                    if (!command.Connection.Ping() || command.Connection.State == System.Data.ConnectionState.Closed)
                    {
                        command.Connection.Open();
                    }
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command.CommandText, command.Connection);
                    adapter.Fill(cas);
                    b = true;
                }
                catch
                {
                    Console.WriteLine(DateTime.Now + "Vrtimo se u petlji SELECT u bazi");
                    Console.WriteLine(DateTime.Now + " " + command.CommandText);
                    b = false;
                }
            }
            return cas;
        }

        public static string randStr(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static DateTime RandomDay()
        {
            DateTime start = new DateTime(2015, 1, 1);
            int range = (DateTime.Today - start).Days;
            return start.AddDays(random.Next(range));



        }
    }
}
