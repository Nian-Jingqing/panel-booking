﻿using DexterLab.Models.Data;
using DexterLab.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace DexterLab.Controllers
{
    public class BookingController : Controller
    {
        // GET: Booking
        public ActionResult Index()
        {
            return View();
        }

        //GET: /Booking/BookPhysicalDevice
        [ActionName("book-physical-device")]
        [HttpGet]
        public ActionResult BookPhysicalDevice()
        {
            return View("BookPhysicalDevice");
        }

        //POST: /Booking/BookPhysicalDevice
        [ActionName("book-physical-device")]
        [HttpPost]
        public ActionResult BookPhysicalDevice(BookingVM model)
        {
            int finalCounter;
            int countDev;
            int elseDev;
            int startCounter = 1;
            int xCounter = 1;
            //Check if Mode State is valid
            if (!ModelState.IsValid)
            {
                return View("BookPhysicalDevice", model);
            }


            using (Db db = new Db())
            {
                //Check if date is unique
                if (!(db.Bookings.Any(x => x.BookingDate.Equals(model.BookingDate))))
                {
                    string email = User.Identity.Name;

                    if (model.DeviceName == "Palo Alto")
                    {

                        countDev = 1;
                    }
                    else
                    {
                        countDev = 2;
                    }

                    //Continue with the booking
                    BookingDTO bookingDTO = new BookingDTO()
                    {
                        DeviceName = model.DeviceName,
                        DeviceSerialNo = model.DeviceSerialNo,
                        DeviceSpace = countDev,
                        BookingDate = model.BookingDate,
                        PanelStart = startCounter,
                        PanelEnd = countDev,
                        BookingPurpose = model.BookingPurpose,
                        ServerInstalled = false,
                        ModifiedBy = "",
                        CreatedBy = email
                    };

                    db.Bookings.Add(bookingDTO);
                    db.SaveChanges();

                    //Check if Identity is equals to the Email Address

                    if (db.Users.Where(a => a.EmailAddress == email).Any())
                    {
                        using (MailMessage mm = new MailMessage())
                        {
                            mm.From = new MailAddress("cruxalphonse@gmail.com");
                            mm.To.Add(email);
                            mm.Subject = "Confirmation for Booking Panel";
                            string body = "Hello,";
                            body += "<br /><br />You have successfully booked Dexter's Lab Panel";
                            body += "<br /> Booking Date: " + model.BookingDate;
                            body += "<br /><br />Panel Booked: Panel " + startCounter + " to Panel " + countDev;
                            body += "<br /><br />Purpose of Booking: " + model.BookingPurpose;
                            body += "<br /><br />Regards, ";
                            body += "<br />NTT Dexter Lab";
                            mm.Body = body;
                            mm.IsBodyHtml = true;

                            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                            {
                                smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                                smtp.EnableSsl = true;
                                smtp.Send(mm);
                            }
                        }
                    }

                }
                else
                {
                    
                    if (model.DeviceName == "Palo Alto")
                    {

                        elseDev = 1;
                    }
                    else
                    {
                        elseDev = 2;
                    }

                    //Check the panel
                    int maxPanel = 30;
                    int ds = elseDev;

                    //While loop for ds != 0
                    while (ds != 0)
                    {
                        //Check if initial panel no is != to PanelStart or PanelEnd
                        if ((!(db.Bookings.Any(x => x.PanelEnd.Equals(xCounter)))) && (!(db.Bookings.Any(x => x.PanelStart.Equals(xCounter)))))
                        {
                            
                            ds--;
                        }
                        else
                        {
                            xCounter++; //Add one in Counter
                            ds = elseDev; //Reset to OG
                                              //if xCounter has reach maxPanel, tempdata error that all panels are book for the or not space are not sufficient for booking
                            if (xCounter > maxPanel)
                            {
                                TempData["Failure"] = "There are no sufficient space for booking any panel on this date.";
                                return View("BookPhysicalDevice", model);
                            }
                            
                        }
                        finalCounter = xCounter;
                    }

                    //Once device space reaches 0 and counter is equals to intial model.DeviceSpace
                    if (ds == 0)
                    {
                        string email = User.Identity.Name;

                        //Continue Booking for DTO
                        BookingDTO booking2DTO = new BookingDTO()
                        {
                            DeviceName = model.DeviceName,
                            DeviceSerialNo = model.DeviceSerialNo,
                            DeviceSpace = elseDev,
                            BookingDate = model.BookingDate,
                            PanelStart = xCounter,
                            PanelEnd = xCounter + (elseDev-1),
                            BookingPurpose = model.BookingPurpose,
                            ServerInstalled = false,
                            ModifiedBy = "",
                            CreatedBy = email
                        };
                        db.Bookings.Add(booking2DTO);
                        db.SaveChanges();
                        //StartPanel = xCounter End Panel = xCounter plus devicespace
                        //Check if Identity is equals to the Email Address
                        
                        if (db.Users.Where(a => a.EmailAddress == email).Any())
                        {
                            int finalCount = xCounter + (elseDev - 1);
                            using (MailMessage mm = new MailMessage())
                            {
                                mm.From = new MailAddress("cruxalphonse@gmail.com");
                                mm.To.Add(email);
                                mm.Subject = "Confirmation for Booking Panel";
                                string body = "Hello,";
                                body += "<br /><br />You have successfully booked Dexter's Lab Panel";
                                body += "<br /> Booking Date: " + model.BookingDate;
                                if (xCounter == finalCount)
                                {
                                    body += "<br /><br />Panel Booked: Panel " + finalCount;
                                }
                                else
                                {
                                    body += "<br /><br />Panel Booked: Panel " + xCounter + " to Panel " + finalCount;
                                }
                                body += "<br /><br />Purpose of Booking: " + model.BookingPurpose ;
                                body += "<br /><br />Regards, ";
                                body += "<br />NTT Dexter Lab";
                                mm.Body = body;
                                mm.IsBodyHtml = true;

                                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                                {
                                    smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                                    smtp.EnableSsl = true;
                                    smtp.Send(mm);
                                }
                            }
                        }

                    }
                }
            }
            //Create a tempdata message
            TempData["Success"] = "You have successfully booked the panel. Check your email for confirmation.";

            //Redirect
            return Redirect("~/Booking/Index");
        }

        //GET: /Booking/MyBooking
        [ActionName("my-bookings")]
        public ActionResult MyBookings()
        {
            

            using (Db db = new Db())
            {
                //Check if user is login
                string email = User.Identity.Name;
                List<MyBookingsVM> myBooking = new List<MyBookingsVM>();

                var booking = db.Bookings.Select(x => new MyBookingsVM
                {
                    Id = x.Id,
                    DeviceName = x.DeviceName,
                    DeviceSerialNo = x.DeviceSerialNo,
                    DeviceSpace = x.DeviceSpace,
                    PanelStart = x.PanelStart,
                    PanelEnd = x.PanelEnd,
                    BookingDate = x.BookingDate,
                    BookingPurpose = x.BookingPurpose,
                    ServerInstalled = x.ServerInstalled,
                    ModifiedBy = x.ModifiedBy,
                    CreatedBy = x.CreatedBy

                }).ToList().Where(x => x.CreatedBy.Equals(email));

                //Init List for BookingVM
                //List<MyBookingsVM> bookings = db.Bookings.Where(x => x.CreatedBy.Equals(email)).Any();
                return View("MyBookings", booking);

            }
            //Check is identity has record in booking table
            //yes, show table
            //no, show "You have no current booking."
        }

        //GET: /Booking/EditBooking/id
        [ActionName("edit-booking")]
        [HttpGet]
        public ActionResult EditBooking(int id)
        {
            int bookingId = id;
            string email = User.Identity.Name;

            //Declare MyBookings VM
            EditBookingsVM model;

            using (Db db = new Db())
            {


                //Get user
                BookingDTO dto = db.Bookings.FirstOrDefault(x => x.Id.Equals(bookingId));

                if (dto == null)
                {
                    TempData["Failure"] = "The booking does not exist";
                    return View("MyBookings");
                }

                model = new EditBookingsVM();
                if (dto.CreatedBy == email)
                {
                    model.Id = dto.Id;
                    model.DeviceSerialNo = dto.DeviceSerialNo;
                    model.BookingPurpose = dto.BookingPurpose;
                    model.ServerInstalled = dto.ServerInstalled;
                    
                }
                else
                {
                    TempData["Failure"] = "Invalid Panel Booking Edit Request";
                    return RedirectToAction("my-bookings");
                }
                

                return View("EditBooking", model);
            }
            
            
        }

        //POST: /Booking/EditBooking/id
        [ActionName("edit-booking")]
        [HttpPost]
        public ActionResult EditBooking(EditBookingsVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditBooking", model);
            }

            string email = User.Identity.Name;

            using (Db db = new Db())
            {
                if(db.Bookings.Any(x => x.Id.Equals(model.Id)))
                {
                    BookingDTO dto = db.Bookings.Find(model.Id);
                    dto.DeviceSerialNo = model.DeviceSerialNo;
                    dto.BookingPurpose = model.BookingPurpose;
                    dto.ServerInstalled = model.ServerInstalled;
                    dto.ModifiedBy = email;
                   
                    db.SaveChanges();
                }
                else
                {
                    TempData["Failure"] = "Invalid Edit Panel Booking Request";
                    return View("EditBooking", model);
                }
            }
            TempData["Success"] = "You have successfully edited your booking";
            return View("EditBooking");
        }

        //GET: /Booking/DeleteBooking/id
        [ActionName("delete-booking")]
        [HttpGet]
        public ActionResult DeleteBooking(int id)
        {
            using (Db db = new Db())
            {
                BookingDTO dto = db.Bookings.Find(id);

                db.Bookings.Remove(dto);

                db.SaveChanges();
            }

            TempData["Success"] = "You have successfully remove your booking";
            return RedirectToAction("my-bookings");
        }
    }
}