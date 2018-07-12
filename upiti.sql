1.SELECT department.department, COUNT(complaints.comment) AS topComplaints FROM department, complaints 
WHERE complaints.department_id = department.id
AND complaints.date >= DATE_SUB(NOW(),INTERVAL 1 YEAR) GROUP BY department.id  
ORDER BY `topComplaints`  DESC LIMIT 1

2.SELECT (SELECT SUM(expenses.sum) FROM expenses WHERE expenses.date BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) 
AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR)) AS rashodi, 
 (SELECT SUM(bill_item.quantity*bill_item.item_price) FROM bill_item WHERE bill_item.date BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) 
 AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR)) AS prihodi, (SELECT(rashodi-prihodi)) AS profit

3.SELECT (SELECT SUM(IF (reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)))*reservation.num_of_guests FROM reservation WHERE reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) AS ukupanBrojNocenja, 
(SELECT ukupanBrojNocenja/3650*100) AS prosecnaGodisnjaPopunjenost, 
MONTHNAME(reservation.arrival) AS mesec, SUM(DATEDIFF(reservation.departure, reservation.arrival))*reservation.num_of_guests/300*100 AS mesecnaPopunjenost FROM reservation WHERE (reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE())AND (reservation.departure BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) GROUP BY mesec order BY MONTH(reservation.arrival)

prosecna godisnja:
SELECT (SELECT SUM(IF (reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)))*reservation.num_of_guests FROM reservation WHERE reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) AS ukupnaGodisnja, (SELECT ukupnaGodisnja/3650 AS prosecnaGodisnja)
prosecna mesecna:
SELECT MONTHNAME(reservation.arrival) AS mesec, SUM(DATEDIFF(reservation.departure, reservation.arrival))*reservation.num_of_guests/300 AS brojNocenja FROM reservation WHERE (reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE())AND (reservation.departure BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) GROUP BY mesec ASC


4.SELECT svakiMesec.prosecnaMesecna, mesec FROM (SELECT MONTHNAME(reservation.arrival) AS 'mesec', SUM(DATEDIFF(reservation.departure, reservation.arrival))*reservation.num_of_guests/300*100 AS 'prosecnaMesecna' 
FROM reservation 
WHERE (reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE())AND (reservation.departure BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) GROUP BY mesec ASC) svakiMesec, (SELECT (SELECT SUM(IF (reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)))*reservation.num_of_guests FROM reservation WHERE reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND CURRENT_DATE()) AS 'ukupnaGodisnja', (SELECT ukupnaGodisnja/3650*100) AS 'prosecnaGodisnja') godisnja WHERE svakiMesec.prosecnaMesecna > godisnja.prosecnaGodisnja order BY MONTH(mesec)

5.SELECT
(SELECT 
SUM(IF(reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival))
)*reservation.num_of_guests 
FROM reservation, occupiedroom, guest 
	WHERE 
reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id AND guest.country = 'SRB') AS domaciNocenja, 
(SELECT 
SUM(IF(reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival)))*reservation.num_of_guests 
FROM reservation, occupiedroom, guest 
	WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id AND guest.country != 'SRB') AS stranciNocenja,
(SELECT 
SUM(IF(reservation.departure <= NOW(), 1, 0)
)*reservation.num_of_guests 
FROM reservation, occupiedroom, guest 
	WHERE 
reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id AND guest.country = 'SRB') AS domaciDolasci, 
(SELECT 
SUM(IF(reservation.departure <= NOW(), 1, 0))*reservation.num_of_guests 
FROM reservation, occupiedroom, guest 
	WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id AND guest.country != 'SRB') AS stranciDolasci


6.SELECT SUM(DATEDIFF(reservation.departure, reservation.arrival))/COUNT(reservation.id) AS prosecnaDuzinaBoravka FROM reservation

7.SELECT agency.name, COUNT(reservation.id) AS brojRezervacija, sum(bill_item.quantity*bill_item.item_price) AS prihodi
FROM bill, bill_item, agency, agency_reservation, reservationtype, reservation, bill_reservation 
WHERE bill_item.bill_id = bill.id 
AND bill_reservation.bill_id = bill.id AND bill_reservation.reservation_id = reservation.id AND reservation.reservationType_id = reservationtype.id AND reservationtype.type = 'Agency' AND agency_reservation.reservation_id = reservation.id AND agency_reservation.agency_id = agency.id AND reservation.arrival 
BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR) GROUP BY agency.name ORDER BY `prihodi` DESC


8.SELECT payment.method, (count(payment.method)/(SELECT count(bill.id) FROM bill, bill_item WHERE bill_item.bill_id = bill.id AND bill_item.date >= DATE_SUB(NOW(),INTERVAL 1 YEAR))*100) AS procenat FROM bill_item, bill, payment WHERE bill_item.bill_id = bill.id AND bill.payment_id = payment.id AND bill_item.date >= DATE_SUB(NOW(),INTERVAL 1 YEAR) GROUP BY payment.method

9.SELECT agency.name, SUM(bill_item.item_price*bill_item.quantity) AS suma FROM bill_item, bill, payment, bill_reservation, reservation, reservationtype, agency_reservation, agency, reservationStatus 
WHERE agency.id = agency_reservation.agency_id AND agency_reservation.reservation_id = reservation.id AND reservation.reservationType_id = 2  
AND reservation.id = bill_reservation.reservation_id AND bill_reservation.bill_id = bill.id AND bill_item.bill_id = bill.id AND payment.id = bill.payment_id 
AND payment.method = 'Neplaceno' AND reservation.reservationStatus_id = reservationStatus.id AND reservationStatus.type = 'Potvrdjena' GROUP BY agency.name

10.
BROJ OTKAZANIH
SELECT COUNT(reservation.id) AS brojOtkazanih FROM reservation, reservationstatus WHERE reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Otkazana' 
AND reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND NOW()
BROJ UKUPNIH
SELECT COUNT(reservation.id) AS brojUkupnih FROM reservation, reservationstatus WHERE reservation.reservationStatus_id = reservationstatus.id AND reservation.arrival 
BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND NOW()
PROCENAT
SELECT ((SELECT COUNT(reservation.id) AS brojOtkazanih FROM reservation, reservationstatus WHERE reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Otkazana' 
AND reservation.arrival BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND NOW())/(SELECT COUNT(reservation.id) AS brojUkupnih FROM reservation, reservationstatus WHERE reservation.reservationStatus_id = reservationstatus.id AND reservation.arrival 
BETWEEN DATE_SUB(NOW(),INTERVAL 1 YEAR) AND NOW()))*100 AS procenat


11.SELECT total.totalRez AS ukupnoOstvarenoRezervacija, otkazane.brrez/total.totalRez*100 AS procenatOtkazanihRezervacija, otkazane.prihodi AS prihodiOdOtkazanih FROM 
(SELECT 
count(reservation.id) AS brRez, 
sum(bill_item.quantity*bill_item.item_price) AS prihodi 
FROM reservation, reservationtype, bill_reservation, agency_reservation, agency, reservationstatus, bill, payment, bill_item 
WHERE bill_item.bill_id = bill.id 
AND bill_reservation.bill_id = bill.id 
AND bill.payment_id = payment.id 
AND payment.method != 'Neplaceno' 
AND bill_reservation.reservation_id = reservation.id 
AND agency_reservation.reservation_id = reservation.id 
AND reservation.reservationStatus_id = reservationstatus.id 
AND reservationstatus.type = 'Otkazana' 
AND reservation.reservationType_id = reservationtype.id 
AND reservationtype.type = 'Agency' 
AND agency_reservation.agency_id = agency.id 
AND agency.name = 'Booking'  
AND reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR)) otkazane,
(SELECT COUNT(reservation.id) AS totalRez FROM reservation, reservationstatus WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena') total


12.SELECT count(occupiedroom.reservation_id) AS dolasci, guest.country FROM reservation, occupiedroom, guest WHERE 
reservation.id = occupiedroom.reservation_id AND occupiedroom.guest_id = guest.id GROUP BY guest.country ORDER BY dolasci DESC

13.SELECT MONTH(reservation.arrival) AS mesec, (count(reservation.id)/(SELECT COUNT(reservation.id) FROM reservation WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR))*100) AS procenat FROM reservation, reservationtype WHERE reservation.reservationType_id = reservationtype.id AND reservationtype.type = 'Web site' AND reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) order BY mesec

14.SELECT 
SUM(bill_reservation.residence_tax_id_tax*DATEDIFF(reservation.departure, reservation.arrival)) AS brojTaksi, 
(SUM(bill_reservation.residence_tax_id_tax*DATEDIFF(reservation.departure, reservation.arrival))*residence_tax.tax_total) AS ukupnaVrednost
FROM 
residence_tax, bill_reservation, bill, bill_item, reservation 
WHERE 
residence_tax.id_tax = bill_reservation.residence_tax_id_tax 
AND bill.id = bill_reservation.bill_id 
AND bill.id = bill_item.bill_id 
AND bill_item.date BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-%m-01'),INTERVAL 1 MONTH) AND LAST_DAY(NOW() - INTERVAL 1 MONTH)

15.SELECT payment.method, SUM(bill_item.item_price*bill_item.quantity) AS prihod FROM bill_item, bill, payment WHERE bill_item.bill_id = bill.id AND bill.payment_id = payment.id AND bill_item.date = CURDATE() GROUP BY payment.method


16.SELECT oveGodine.brojNocenja as 'oveGodine', prosleGodine.brNocenja as 'prosleGodine', oveGodine.brojNocenja-prosleGodine.brNocenja AS 'razlika'
FROM 
(SELECT	(SUM(IF(reservation.departure <= LAST_DAY(NOW() - INTERVAL 1 MONTH), 
			DATEDIFF(reservation.departure, reservation.arrival), 
			DATEDIFF(LAST_DAY(NOW() - INTERVAL 1 MONTH), reservation.arrival))
*reservation.num_of_guests)) AS 'brojNocenja' 
FROM reservation 
WHERE reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-%m-01'),INTERVAL 1 MONTH) AND LAST_DAY(NOW() - INTERVAL 1 MONTH)) oveGodine,
 (SELECT 
 	(SUM(
 		IF(reservation.departure <= DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR), 
 			DATEDIFF(reservation.departure, reservation.arrival), 
 			DATEDIFF(DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR), reservation.arrival)))*reservation.num_of_guests) AS 'brNocenja' FROM reservation WHERE (reservation.arrival BETWEEN DATE_SUB(DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-%m-01'),INTERVAL 1 MONTH), INTERVAL 1 YEAR) AND DATE_SUB(LAST_DAY(NOW() - INTERVAL 1 MONTH), INTERVAL 1 YEAR))) prosleGodine


17.SELECT total.ukupno as 'ukupnoNocenjaProsleGodine', prosleGodine.viseOdDeset as 'BrojGostijuSaViseOdDesetNocenja', prosleGodine.viseOdDeset/total.ukupno AS procenat FROM 
(SELECT COUNT(brojNocenjaPoGostu.brojNocenja) AS 'viseOdDeset' 
FROM (SELECT SUM(IF(reservation.departure <= NOW(), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(NOW(), reservation.arrival))*reservation.num_of_guests) AS brojNocenja FROM reservation, reservationstatus, occupiedroom, guest WHERE reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena' AND occupiedroom.reservation_id = reservation.id AND occupiedroom.guest_id = guest.id GROUP BY guest.id) brojNocenjaPoGostu 
WHERE brojNocenjaPoGostu.brojNocenja>10) prosleGodine,
(SELECT (SUM(
IF(reservation.departure <= DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR), DATEDIFF(reservation.departure, reservation.arrival), DATEDIFF(DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR), reservation.arrival))
)*reservation.num_of_guests) AS 'ukupno'
FROM reservation, reservationstatus WHERE reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena') total


18.SELECT 
MONTH(reservation.arrival) AS mesec, 
(count(reservation.id)/(SELECT COUNT(reservation.id) FROM reservation,reservationstatus WHERE reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) AND reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena')*100) AS procenat
 FROM reservation, reservationstatus, occupiedroom, room 
WHERE reservation.reservationStatus_id = reservationstatus.id AND reservationstatus.type = 'Potvrdjena' AND reservation.id = occupiedroom.reservation_id AND occupiedroom.room_id = room.id AND room.smoking = 'Y' AND reservation.arrival >= DATE_SUB(NOW(),INTERVAL 1 YEAR) order BY mesec

19.SELECT 
tekuceTromesecje.brojDana as 'Tromesecje ove godine', 
prosloTromesecje.brojDana as 'Tromesecje prosle godine' 
FROM
 (SELECT SUM(bill_item.quantity) AS brojDana FROM services, bill_item WHERE services.type = 'CONFERENCE ROOM' AND services.id = bill_item.services_id_service AND bill_item.date BETWEEN DATE_FORMAT(CURRENT_DATE,'%Y-01-01') AND DATE_FORMAT(CURRENT_DATE,'%Y-03-31')) tekuceTromesecje, 
(SELECT SUM(bill_item.quantity) AS brojDana FROM services, bill_item WHERE services.type = 'CONFERENCE ROOM' AND services.id = bill_item.services_id_service AND bill_item.date BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-03-31'),INTERVAL 1 YEAR)) prosloTromesecje


20.SELECT agency.industry,
DATEDIFF(reservation.departure, reservation.arrival)*reservation.num_of_guests AS brojNocenja
FROM 
agency, agency_reservation, reservationtype, reservation, reservationstatus
WHERE 
reservation.reservationType_id = 2 AND 
reservation.reservationStatus_id = 1 AND 
agency_reservation.reservation_id = reservation.id AND agency_reservation.agency_id = agency.id 
AND reservation.arrival BETWEEN DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-01-01'),INTERVAL 1 YEAR) AND DATE_SUB(DATE_FORMAT(CURRENT_DATE,'%Y-12-31'),INTERVAL 1 YEAR)
GROUP BY agency.industry ORDER BY brojNocenja DESC

