using System.Web.Mvc;
using Zeus.Templates.Mvc.Controllers;
using Zeus.Web;
using Zeus.AddIns.ECommerce.PaypalExpress.Mvc.ViewModels;
using Zeus.AddIns.ECommerce.PaypalExpress.Mvc.ContentTypeInterfaces;
using System.Linq;
using System;
using System.Web;

namespace Zeus.AddIns.ECommerce.PaypalExpress.Mvc.Controllers
{
    /// <summary>
    /// For this to work you need to Implement the following spark files : PayPalConfirmation, CheckoutFailed,  CheckoutSuccessful, EmailFailed, PayPalFailed, PayPalReturnedWrongCountry
    /// </summary>
    /// <typeparam name="T">ContentItem which implements IPayPalBasketPage</typeparam>
    public abstract class PayPalBasketPageController<T> : ZeusController<T> where T : ContentItem, IPayPalBasketPage
    {
        public virtual ActionResult Checkout()
        {
            var basketPageViewModel = GetViewModel(CurrentItem);

            var ReturnUrl = System.Web.HttpContext.Current.Request.Url.Host + CurrentItem.BasketPagePath;

            string token = null;
            string retMsg = null;
            var amt = CurrentItem.BasketTotal.ToString().TrimEnd('0');

            //pass in the amount to take here, as well as the yay (success) and boo (failure) pages
            var payPalCaller = new NVPAPICaller();
            var ret = payPalCaller.ShortcutExpressCheckout(amt, ref token, ref retMsg,
                                                            ReturnUrl + "/PayPalConfirmation",
                                                            ReturnUrl + "/CheckoutFailed",
                                                            CurrentItem.BasketItems,
                                                            CurrentItem.DeliveryPrice,
                                                            CurrentItem.Currency,
                                                            ForceReturnURLsOverHttps,
                                                            CurrentItem.TaxTotal
                                                            );
            if (ret)
            {
                System.Web.HttpContext.Current.Session["token"] = token;
                System.Web.HttpContext.Current.Session["payment_amt"] = CurrentItem.BasketTotal;
                //this kicks it to the correct PayPal page in order to take the payment
                System.Web.HttpContext.Current.Response.Redirect(retMsg);
                return null;
            }
            else
            {
                basketPageViewModel.PaymentReturnMessage = retMsg;
                //the payment failed
                //code here should display the error message - this is not a payment error, this is an account error, 
                //or an error with the totals being passed to PayPal - the rounding should be done from the calling application rather than inside this code!!!
                basketPageViewModel.CheckoutMessage = retMsg;

                return View("PayPalFailed", basketPageViewModel);
            }
        }

        public virtual bool ForceReturnURLsOverHttps { get { return false; } }

        public virtual Type typeOfViewModel { get { return typeof(PayPalBasketPageViewModel<T>); } }

        public virtual ActionResult CheckoutSuccessful()
        {
            var test = new NVPAPICaller();

            var retMsg = "";
            var token = "";
            var finalPaymentAmount = "";
            var payerId = "";
            var decoder = new NVPCodec();

            token = System.Web.HttpContext.Current.Session["ppToken"].ToString();
            payerId = System.Web.HttpContext.Current.Session["ppID"].ToString();
            finalPaymentAmount = CurrentItem.BasketTotal.ToString().TrimEnd('0');

            var ret = test.ConfirmPayment(finalPaymentAmount, token, payerId, ref decoder, ref retMsg, CurrentItem.Currency);

            if (ret)
            {
                var basketPageViewModel = GetViewModel(CurrentItem);
                var shippingAddress = (Address)System.Web.HttpContext.Current.Session["shippingAddress"];
                var noteToSeller = (string)System.Web.HttpContext.Current.Session["ppNoteToSeller"];

                try
                {
                    PayPalOrderSuccess(basketPageViewModel, shippingAddress, token);
                    basketPageViewModel.ShippingAddress = shippingAddress;
                    basketPageViewModel.NoteToSeller = noteToSeller;
                    return View(basketPageViewModel);
                }
                catch (System.Exception ex)
                {
                    // log error in system event viewer
                    //Logger.PayPalCheckoutOrderError(ex, token, Basket.GetBasket(), shippingAddress);

                    // show error view
                    basketPageViewModel.OrderProcessingErrorMessage = ex.Message;
                    return View("EmailFailed", basketPageViewModel);
                }
            }
            else
            {
                var basketPageViewModel = GetViewModel(CurrentItem);
                basketPageViewModel.CheckoutMessage = retMsg;

                return View("PayPalFailed", basketPageViewModel);
            }
        }

        public virtual void PayPalOrderSuccess(IPayPalBasketPageViewModel viewModel, Address shippingAddress, string token)
        {
            //This needs to be overridden...
            /*
            var initBasketItems = basketPageViewModel.ItemsInBasket;

            // create order
            var processor = new CheckoutProcessor();
            var orderReference = processor.ProcessOrder(_basket, shippingAddress, shippingAddress, "PayPalExpress", token, "");

            // send confirmation e-mail
            EmailNotifyer.SendConfirmationEmail(Basket.GetBasket(), shippingAddress);

            // show the view
            basketPageViewModel.OrderReference = orderReference;
            basketPageViewModel.ShippingAddress = shippingAddress;
            return View(basketPageViewModel);
             */
        }

        [HttpGet]
        public virtual ActionResult PayPalConfirmation(string token, string PayerID)
        {
            //get shipping address
            var test = new NVPAPICaller();

            var shippingAddress = new Address();
            var retMsg = "";
            var noteToSeller = "";
            var pass = test.GetShippingDetails(token, ref PayerID, ref shippingAddress, ref noteToSeller, ref retMsg);

            if (pass)
            {
                System.Web.HttpContext.Current.Session["shippingAddress"] = shippingAddress;
                //System.Web.HttpContext.Current.Session[""] = shippingAddress;
                System.Web.HttpContext.Current.Session["ppNoteToSeller"] = noteToSeller;
                System.Web.HttpContext.Current.Session["ppToken"] = token;
                System.Web.HttpContext.Current.Session["ppID"] = PayerID;

                //at this point check to see if an appropriate country has been used...
                var basketPageViewModel = GetViewModel(CurrentItem);
                basketPageViewModel.ShippingAddress = shippingAddress;

                if (CurrentItem.ForceCountryMatch && !CurrentItem.PossibleCountries.Contains(shippingAddress.Country))
                {
                    //throw the error...
                    return View("PayPalReturnedWrongCountry", basketPageViewModel);
                }
                else if (CurrentItem.ForceStateMatch && !CurrentItem.PossibleStates.Contains(shippingAddress.StateRegion))
                {
                    //throw the error...
                    return View("PayPalReturnedWrongState", basketPageViewModel);
                }

                var result = View(basketPageViewModel);

                return result;
            }
            else
            {
                var basketPageViewModel = GetViewModel(CurrentItem);
                basketPageViewModel.CheckoutMessage = retMsg;

                return View("PayPalFailed", basketPageViewModel);
            }
        }

        public ActionResult CheckoutFailed(string token)
        {
            var basketPageViewModel = GetViewModel(CurrentItem);
            var result = View(basketPageViewModel);
            return result;
        }

        public virtual IPayPalBasketPageViewModel GetViewModel(T CurrentItem)
        {
            return new PayPalBasketPageViewModel<T>(CurrentItem);
        }
    }
}
