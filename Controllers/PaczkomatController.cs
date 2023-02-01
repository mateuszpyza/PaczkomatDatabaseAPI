using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaczkomatDatabaseAPI.Models;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop.Infrastructure;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel.DataAnnotations;

namespace PaczkomatDatabaseAPI.Controllers
{
    [ApiController]
    [Route("api/paczkomat")]
    public class PaczkomatController : ControllerBase
    {
        private readonly PaczkomatDbContext _paczkomatDbContext;
        private readonly IMapper _mapper;
        private readonly Random rnd = new ();
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IPasswordHasher<Models.Machine> _passwordHasherMachine;
        private readonly AuthenticationSettings _authenticationSettings;

        public PaczkomatController(PaczkomatDbContext paczkomatDbContext, IPasswordHasher<User> passwordHasher, IPasswordHasher<Models.Machine> passwordHasherMachine, IMapper mapper, AuthenticationSettings authenticationSettings)
        {
            _paczkomatDbContext = paczkomatDbContext;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _passwordHasherMachine = passwordHasherMachine;
            _authenticationSettings = authenticationSettings;
        }

        // tylko do testów
        [HttpGet("users")]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {   
            var users = _paczkomatDbContext.Users.ToList();
            return Ok(users);
        }

        // tylko do testów
        [HttpGet("orders")]
        public ActionResult<IEnumerable<Order>> GetAllOrders()
        {
            var orders = _paczkomatDbContext.Orders.ToList();
            return Ok(orders);
        }

        // Paczki które odbiera konkretny użytkownik [WYKONANE]
        [HttpGet("orders/receiving/{phoneNumber}")]
        [Authorize(Roles = "admin,user")]
        public ActionResult<IEnumerable<Order>> GetOrdersToReceive([FromRoute] int phoneNumber)
        {
            var orders = _paczkomatDbContext.Orders
                .Where(o => o.ReceiverUser == phoneNumber)
                .Select(o => new {o.Id, o.Status, o.SenderUser, o.CodeReceiving, o.ReceiverMachine, o.DeliveryDate, o.ReceivingDate})
                .ToList();

            if (!orders.IsNullOrEmpty())
                return Ok(orders);
            else
                if (!_paczkomatDbContext.Users.Where(o => o.PhoneNumber == phoneNumber).IsNullOrEmpty())
                    return NotFound("Jeszcze nie było paczek do odbioru.");
                else
                    return NotFound("Taki użytkownik nie istnieje!");
        }

        // Paczki które wysyła konkretny użytkownik [WYKONANE]
        [Authorize(Roles = "admin,user")]
        [HttpGet("orders/sending/{phoneNumber}")]
        public ActionResult<IEnumerable<Order>> GetOrdersToSend([FromRoute] int phoneNumber)
        {
            var orders = _paczkomatDbContext.Orders
                .Where(o => o.SenderUser == phoneNumber)
                .Select(o => new { o.Id, o.Status, o.ReceiverUser, o.CodeInserting, o.SenderMachine, o.InsertionDate , o.DeliveryDate})
                .ToList();

            if (!orders.IsNullOrEmpty())
                return Ok(orders);
            else
                if (!_paczkomatDbContext.Users.Where(o => o.PhoneNumber == phoneNumber).IsNullOrEmpty())
                return NotFound("Jeszcze nie wysłałeś żadnej paczki.");
            else
                return NotFound("Taki użytkownik nie istnieje!");
        }

        // Paczki do odebrania przez kuriera [WYKONANE]
        [HttpGet("orders/picking")]
        [Authorize(Roles = "admin,delivery_man")]
        public ActionResult<IEnumerable<Order>> GetInserted()
        {
            var orders = _paczkomatDbContext.Orders
                .Where(o => o.Status == "inserted")
                .Select(o => new { o.Id, o.Status, o.SenderMachine, o.InsertionDate, o.CodePicking })
                .ToList();

            if (!orders.IsNullOrEmpty())
                return Ok(orders);
            else
                return NotFound("Nie ma już paczek do odebrania.");
        }

        // Paczki do dostarczenia przez kuriera [WYKONANE]
        [HttpGet("orders/delivering")]
        [Authorize(Roles = "admin,delivery_man")]
        public ActionResult<IEnumerable<Order>> GetPicked()
        {
            var orders = _paczkomatDbContext.Orders
                .Where(o => o.Status == "picked")
                .Select(o => new { o.Id, o.Status, o.ReceiverMachine, o.PickingDate, o.CodeDelivering })
                .ToList();

            if (!orders.IsNullOrEmpty())
                return Ok(orders);
            else
                return NotFound("Nie ma już paczek do dostarczenia.");
        }

        // Wszystkie informacje o pojedynczym użytkowniku [TRYB TYLKO TESTOWY]
        [HttpGet("user/{phoneNumber}")]
        public ActionResult<User> GetUser([FromRoute] int phoneNumber)
        { 
            var user = _paczkomatDbContext.Users.Where(u => u.PhoneNumber == phoneNumber).FirstOrDefault();

            if (user is not null)
                return Ok(user);
            else
                return NotFound("Nie odnaleziono takiego użytkownika!");
        }
       
        // KONIEC GET-ÓW

        // Logowanie za pomocą numeru telefonu [WYKONANE]
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginDto loginDto)
        {
            //var hashedPassword = _passwordHasher.HashPassword(user, loginDto.Password);
            //user.Password = hashedPassword;

            var user = _paczkomatDbContext.Users
                .Where(a => a.PhoneNumber == loginDto.PhoneNumber)
                .FirstOrDefault();

            if (user is null)
                return NotFound("Nie odnaleziono takiej kombinacji numeru telefonu i hasła.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, loginDto.Password);

            if(result == PasswordVerificationResult.Failed)
                return NotFound("Nie odnaleziono takiej kombinacji numeru telefonu i hasła.");

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.PhoneNumber.ToString()),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.Surname}"),
                new Claim(ClaimTypes.Role, user.AccountType),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);
            var tokenHandler = new JwtSecurityTokenHandler();
            
            return Ok(new { phoneNumber = user.PhoneNumber, accountType = user.AccountType, token = tokenHandler.WriteToken(token)});
        }

        // sprawdzenie czy paczkomat może obsłużyć wkładanie paczki przez klienta [WYKONANE]
        [HttpPost("{machineId}/package/insertion")]
        public ActionResult<Order> CheckInsertion([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.SenderUser == phoneCode.PhoneNumber)
                .Where(o => o.CodeInserting == phoneCode.Code)
                .Where(o => o.Status == "ordered")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiej paczki do włożenia!");

            var package = _paczkomatDbContext.Packages.Where(p => p.OrderId == order.Id).FirstOrDefault();

            if (package is null)
                return Conflict("Zamówienie nie ma przypisanej paczki!");

            var machine = _paczkomatDbContext.Machines
                            .Where(m => m.Id == machineId)
                            .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "free").FirstOrDefault();

            if (package.Size == "bigger")
                locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "free")
                        .Where(l=> l.Size == package.Size).FirstOrDefault();

            if (locker is null)
                return NotFound("Nie ma miejsca w paczkomacie!");

            return Ok(new { locker = locker.Id, order = order.Id});

        }

        // potwierdzenie zakończenia wkładania paczki do paczkomatu przez klienta, zapis zmian w bazie [WYKONANE]
        [HttpPost("{machineId}/{lockerId}/package/{orderId}/inserted")]
        public ActionResult SaveInsertion([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId, string lockerId, int orderId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.Id == orderId)
                .Where(o => o.SenderUser == phoneCode.PhoneNumber)
                .Where(o => o.CodeInserting == phoneCode.Code)
                .Where(o => o.Status == "ordered")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia!");

            var package = _paczkomatDbContext.Packages.Where(p => p.OrderId == order.Id).FirstOrDefault();

            if (package is null)
                return Conflict("Zamówienie nie ma przypisanej paczki!");

            var machine = _paczkomatDbContext.Machines
                           .Where(m => m.Id == machineId)
                           .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.Id == lockerId)
                        .Where(l => l.State == "free").FirstOrDefault();

            if (package.Size == "bigger")
                locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.Id == lockerId)
                        .Where(l => l.State == "free")
                        .Where(l => l.Size == package.Size).FirstOrDefault();
           
            if (locker is null)
                return NotFound("Coś jest nie tak ze skrytką!");

            int codeGenerated;

            do
            {
               codeGenerated = rnd.Next(100000, 999999);
            } while (!_paczkomatDbContext.Orders.Where(o => o.CodePicking == codeGenerated)
                                                .Where(o => o.Status == "inserted").IsNullOrEmpty());

            order.Status = "inserted";                                    
            order.SenderMachine = machineId;
            order.SenderLocker = lockerId;
            order.CodePicking = codeGenerated;
            order.InsertionDate = DateTime.Now;

            locker.State = "taken";

            _paczkomatDbContext.SaveChanges();
            return Ok("Udało się zakończyć wkładanie paczki.");
         
        }

        // sprawdzenie czy paczkomat może obsłużyć odbieranie paczki przez kuriera [WYKONANE]
        [HttpPost("{machineId}/package/picking")]
        public ActionResult<Order> CheckPicking([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId)
        {
            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == phoneCode.PhoneNumber)
                                .Where(o => o.AccountType == "delivery_man")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do kuriera!");

            var order = _paczkomatDbContext.Orders
                          .Where(o => o.CodePicking == phoneCode.Code)
                          .Where(o => o.Status == "inserted")
                          .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia");

            var machine = _paczkomatDbContext.Machines
                           .Where(m => m.Id == machineId)
                           .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "taken")
                        .Where(l => l.Id == order.SenderLocker)
                        .FirstOrDefault();

            if (locker is not null)
                return Ok(new { locker = locker.Id, order = order.Id });
            else
                return NotFound("Nie ma takiej paczki do wyciągnięcia!");
        }

        // potwierdzenie odebrania paczki przez kuriera [WYKONANE]
        [HttpPost("{machineId}/{lockerId}/package/{orderId}/picked")]
        public ActionResult SavePicking([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId, string lockerId, int orderId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.Id == orderId)
                .Where(o => o.CodePicking == phoneCode.Code)
                .Where(o => o.Status == "inserted")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "taken")
                        .Where(l => l.Id == lockerId)
                        .FirstOrDefault();

            if(locker is null)
                return NotFound("Wystąpił problem ze skrytką!");

            var machine = _paczkomatDbContext.Machines
                           .Where(m => m.Id == machineId)
                           .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == phoneCode.PhoneNumber)
                                .Where(o => o.AccountType == "delivery_man")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do kuriera!");

            int codeGenerated;

            do
            {
               codeGenerated = rnd.Next(100000, 999999);
            } while (!_paczkomatDbContext.Orders.Where(o => o.CodeDelivering == codeGenerated)
                                                    .Where(o => o.Status == "picked").IsNullOrEmpty());
            order.Status = "picked";
            order.CodeDelivering = codeGenerated;
            order.PickingDate = DateTime.Now;

            locker.State = "free";

            _paczkomatDbContext.SaveChanges();
            return Ok("Udało się zakończyć wyciąganie paczki.");
        }

        // sprawdzenie czy paczkomat może przyjąć paczkę od kuriera [WYKONANE]
        [HttpPost("{machineId}/package/delivering")]
        public ActionResult<Order> CheckDelivering([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId)
        {
            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == phoneCode.PhoneNumber)
                                .Where(o => o.AccountType == "delivery_man")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do kuriera!");

            var order = _paczkomatDbContext.Orders
                          .Where(o => o.CodeDelivering == phoneCode.Code)
                          .Where(o => o.Status == "picked")
                          .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia");

            var package = _paczkomatDbContext.Packages.Where(p => p.OrderId == order.Id).FirstOrDefault();

            if (package is null)
                return Conflict("Zamówienie nie ma przypisanej paczki!");

            var machine = _paczkomatDbContext.Machines
                           .Where(m => m.Id == machineId)
                           .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "free").FirstOrDefault();

            if (package.Size == "bigger")
                locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "free")
                        .Where(l => l.Size == package.Size).FirstOrDefault();

            if (locker is null)
                return NotFound("Nie ma miejsca w paczkomacie!");

            return Ok(new { locker = locker.Id, order = order.Id });    
        }

        // potwierdzenie że kurier zakończył umieszczanie paczki w paczkomacie [WYKONANE]
        [HttpPost("{machineId}/{lockerId}/package/{orderId}/delivered")]
        public ActionResult SaveDelivering([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId, string lockerId, int orderId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.Id == orderId)
                .Where(o => o.CodeDelivering == phoneCode.Code)
                .Where(o => o.Status == "picked")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia");

            var package = _paczkomatDbContext.Packages.Where(p => p.OrderId == order.Id).FirstOrDefault();

            if (package is null)
                return Conflict("Zamówienie nie ma przypisanej paczki!");

            var machine = _paczkomatDbContext.Machines
                          .Where(m => m.Id == machineId)
                          .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.Id == lockerId)
                        .Where(l => l.State == "free").FirstOrDefault();

            if (package.Size == "bigger")
                locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.Id == lockerId)
                        .Where(l => l.State == "free")
                        .Where(l => l.Size == package.Size).FirstOrDefault();

            if (locker is null)
                return NotFound("Coś jest nie tak ze skrytką!");

            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == phoneCode.PhoneNumber)
                                .Where(o => o.AccountType == "delivery_man")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do kuriera!");

           
            int codeGenerated;

            do
            {
                codeGenerated = rnd.Next(100000, 999999);
            } while (!_paczkomatDbContext.Orders.Where(o => o.CodeReceiving == codeGenerated)
                                                .Where(o => o.Status == "delivered").IsNullOrEmpty());
            order.Status = "delivered";
            order.ReceiverLocker = lockerId; 
            order.CodeReceiving = codeGenerated;
            order.DeliveryDate = DateTime.Now;

            locker.State = "taken";

            _paczkomatDbContext.SaveChanges();
            return Ok("Udało się zakończyć umieszczanie paczki.");      
        }

        // sprawdzenie czy użytkownik może odebrać paczkę z paczkomatu [WYKONANE]
        [HttpPost("{machineId}/package/receiving")]
        public ActionResult<Order> CheckReceiving([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.ReceiverUser == phoneCode.PhoneNumber)
                .Where(o => o.CodeReceiving == phoneCode.Code)
                .Where(o => o.Status == "delivered")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia!");

            var machine = _paczkomatDbContext.Machines
                         .Where(m => m.Id == machineId)
                         .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "taken")
                        .Where(l => l.Id == order.ReceiverLocker)
                        .FirstOrDefault();

            if (locker is not null)
                return Ok(new { locker = locker.Id, order = order.Id });
            else
                return NotFound("Wystąpił problem ze skrytką!");
        }

        // potwierdzenie odebrania paczki z paczkomatu przez użytkownika
        [HttpPost("{machineId}/{lockerId}/package/{orderId}/received")]
        public ActionResult SaveReceiving([FromBody] PhoneCodeDto phoneCode, [FromRoute] string machineId, string lockerId, int orderId)
        {
            var order = _paczkomatDbContext.Orders
                .Where(o => o.Id == orderId)
                .Where(o => o.ReceiverUser == phoneCode.PhoneNumber)
                .Where(o => o.CodeReceiving == phoneCode.Code)
                .Where(o => o.Status == "delivered")
                .FirstOrDefault();

            if (order is null)
                return NotFound("Nie ma takiego zamówienia!");

            var machine = _paczkomatDbContext.Machines
                         .Where(m => m.Id == machineId)
                         .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, phoneCode.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var locker = _paczkomatDbContext.Lockers
                        .Where(l => l.MachineId == machineId)
                        .Where(l => l.State == "taken")
                        .Where(l => l.Id == lockerId)
                        .FirstOrDefault();

            if (locker is null)
                return NotFound("Wystąpił problem ze skrytką!");

            order.Status = "received";
            order.ReceivingDate = DateTime.Now;

            locker.State = "free";

            _paczkomatDbContext.SaveChanges();
            return Ok("Udało się zakończyć odbieranie paczki.");   
        }

        // Zgłaszanie awarii w paczkomacie [WYKONANE]
        [HttpPost("failure/report")]
        public ActionResult ReportFailure([FromBody] MachineEventDto reportFailureDto)
        {
            var machine = _paczkomatDbContext.Machines.Where(u => u.Id == reportFailureDto.Machine).FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, reportFailureDto.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            int newFailureId;
            var failureRecord = _paczkomatDbContext.Failures.OrderBy(f => f.Id).LastOrDefault();

            if (failureRecord is null)
                newFailureId = 1;
            else
                newFailureId = failureRecord.Id + 1;


            var newFailure = _mapper.Map<Failure>(reportFailureDto, opt =>
            {
                opt.Items["Id"] = newFailureId;
                opt.Items["OccurDate"] = DateTime.Now;
            });

            machine.Status = "broken";
            _paczkomatDbContext.Add(newFailure);
            _paczkomatDbContext.SaveChanges();

            return Created($"/api/paczkomat/failures/{newFailureId}", "Awaria zgłoszona.");
        }

        // Zgłaszanie naprawienia awarii paczkomatu [WYKONANE]
        [HttpPost("failure/{failureId}/fixed")]
        [Authorize(Roles = "admin,serviceman")]
        public ActionResult FailureFixed([FromRoute] int failureId, [FromBody] MachineEventDto fixedFailureDto)
        {
            var failureRecord = _paczkomatDbContext.Failures.Where(f => f.Id == failureId).FirstOrDefault();

            if (failureRecord is null)
                return NotFound("Nie znaleziono awarii o takim ID!");

            if (failureRecord.FixDate is not null)
                return Conflict("Ta awaria już została naprawiona!");

            var machine = _paczkomatDbContext.Machines.Where(u => u.Id == fixedFailureDto.Machine).FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, fixedFailureDto.MachinePassword.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            machine.Status = "normal";

            failureRecord.FixDate = DateTime.Now;

            _paczkomatDbContext.SaveChanges();

            return Ok("Udało się zgłosić naprawę awarii.");
        }

        // Zgłaszanie rozpoczęcia przeglądu na paczkomacie [WYKONANE]
        [HttpPost("inspection/start")]
        public ActionResult<Inspection> StartInspection([FromBody] InspectionEventDto inspectionDto)
        {
            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == inspectionDto.PhoneNumber)
                                .Where(u => u.AccountType == "serviceman")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do serwisanta!");

            var machine = _paczkomatDbContext.Machines
                            .Where(m => m.Id == inspectionDto.Machine)
                            .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, inspectionDto.Code.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");
            

            if (machine.Status == "inspection")
                return Conflict("Na tym paczkomacie jest już prowadzony przegląd!"); 

            int newInspectionId;
            var inspectionRecord = _paczkomatDbContext.Inspections.OrderBy(i => i.Id).LastOrDefault();

            if (inspectionRecord is null)
                newInspectionId = 1;
            else
                newInspectionId = inspectionRecord.Id + 1;


            var newInspection = new Inspection
            {
                Id = newInspectionId,
                Description = inspectionDto.Description,
                MachineId = inspectionDto.Machine,
                Serviceman = inspectionDto.PhoneNumber,
                StartDate = DateTime.Now
            };

            machine.Status = "inspection";

            _paczkomatDbContext.Add(newInspection);
            _paczkomatDbContext.SaveChanges();

            return Ok(new { newInspection.Id });
        }

        // Zgłaszanie zakończenia przeglądu na paczkomacie [WYKONANE]
        [HttpPost("inspection/{inspectionId}/ended")]
        public ActionResult EndInspection([FromRoute] int inspectionId, [FromBody] InspectionEventDto inspectionDto)
        {
            var delivery_man = _paczkomatDbContext.Users
                                .Where(u => u.PhoneNumber == inspectionDto.PhoneNumber)
                                .Where(u => u.AccountType == "serviceman")
                                .FirstOrDefault();

            if (delivery_man is null)
                return NotFound("Ten numer nie należy do serwisanta!");

            var machine = _paczkomatDbContext.Machines
                            .Where(m => m.Id == inspectionDto.Machine)
                            .FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            var result = _passwordHasherMachine.VerifyHashedPassword(machine, machine.Password, inspectionDto.Code.ToString());

            if (result == PasswordVerificationResult.Failed)
                return NotFound("Taki paczkomat nie istnieje lub podano błędne hasło!");

            if (machine.Status != "inspection")
                return Conflict("Na tym paczkomacie nie jest prowadzony przegląd!");

            var inspectionRecord = _paczkomatDbContext.Inspections.Where(i => i.Id == inspectionId).FirstOrDefault();

            if (inspectionRecord is null)
                return NotFound("Nie znaleziono przeglądu o takim ID!");

            if (inspectionRecord.EndDate is not null)
                return Conflict("Ten przegląd już został zakończony!");

            machine.Status = "normal";
            inspectionRecord.EndDate = DateTime.Now;

            _paczkomatDbContext.SaveChanges();

            return Ok("Udało się zgłosić zakończenie przeglądu.");
        }

        // Dodawanie nowego zamówienia, z wyborem maksymalnego rozmiaru paczki [WYKONANE]
        [HttpPost("create/order")]
        [Authorize(Roles = "admin,user")]
        public ActionResult CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var senderUser =_paczkomatDbContext.Users.Where(u => u.PhoneNumber == createOrderDto.SenderUser).FirstOrDefault();

            if (senderUser is null)
                return NotFound("Takiego nadawcy nie mamy w bazie!");

            var receiverUser = _paczkomatDbContext.Users.Where(u => u.PhoneNumber == createOrderDto.ReceiverUser).FirstOrDefault();

            if (receiverUser is null)
                return NotFound("Takiego odbiorcy nie mamy w bazie, nie otrzyma kodu odbioru!");

            var machine = _paczkomatDbContext.Machines.Where(u => u.Id == createOrderDto.ReceiverMachine).FirstOrDefault();

            if (machine is null)
                return NotFound("Taki paczkomat nie istnieje!");

            if (createOrderDto.Size != "smaller" && createOrderDto.Size != "bigger")
                return BadRequest("Niepoprawny format rozmiaru paczki!");

            int newOrderId;
            var orderRecord = _paczkomatDbContext.Orders.OrderBy(i => i.Id).LastOrDefault();

            if (orderRecord is null)
                newOrderId = 1;
            else
                newOrderId = orderRecord.Id + 1;

            int code;

            do
            {
                code = rnd.Next(100000, 999999);
            } while (!_paczkomatDbContext.Orders.Where(o => o.SenderUser == createOrderDto.SenderUser)
                                                .Where(o => o.CodeInserting == code)
                                                .Where(o => o.Status == "ordered").IsNullOrEmpty());
            

            //Mapowanie z forsowaniem typu konta "user"
            var order = _mapper.Map<Order>(createOrderDto, opt => 
            {   
                opt.Items["Id"] = newOrderId;
                opt.Items["CodeInserting"] = code;
                opt.Items["OrderDate"] =  DateTime.Now;
                opt.Items["Status"] = "ordered";
            });

            _paczkomatDbContext.Add(order);

            // budowanie rekordu paczki

            int newPackageId;
            var packageRecord = _paczkomatDbContext.Packages.OrderBy(i => i.Id).LastOrDefault();

            if (packageRecord is null)
                newPackageId = 1;
            else
                newPackageId = packageRecord.Id + 1;

            var newPackage = new Package
            {
                Id = newPackageId,
                OrderId = newOrderId,
                Description = createOrderDto.Description,
                Weight = createOrderDto.Weight,
                Size = createOrderDto.Size,
            };

            _paczkomatDbContext.Add(newPackage);

            _paczkomatDbContext.SaveChanges();

            return Created($"/api/paczkomat/order/{newOrderId}", "Dodano zamówienie.");
        }

        // Dodawanie użytkownika, z opcjonalnym podaniem jego adresu [WYKONANE]
        [HttpPost("create/user")]
        [Authorize(Roles = "admin,user")]
        public ActionResult CreateUser([FromBody] CreateUserDto newUserDto)
        {
            //jeśli będzie dodawany adres 
            if (newUserDto.Country != null && newUserDto.Province != null && newUserDto.Town != null && newUserDto.PostalCode != null && newUserDto.AddressNumber != null)
            {
                var possiblyExistingAddress = _paczkomatDbContext.Addresses.Where(a => a.Country == newUserDto.Country)
                                                                           .Where(a => a.Province == newUserDto.Province)
                                                                           .Where(a => a.Town == newUserDto.Town)
                                                                           .Where(a => a.PostalCode == newUserDto.PostalCode)
                                                                           .Where(a => a.Street == newUserDto.Street)
                                                                           .Where(a => a.AddressNumber == newUserDto.AddressNumber)
                                                                            .FirstOrDefault();
                // jeśli taki adres już istnieje
                if (possiblyExistingAddress is not null)
                {
                    newUserDto.AddressId = possiblyExistingAddress.Id;
                }
                // kiedy musimy utworzyć nowy adres
                else
                {
                    int newAddressId;
                    var addressRecord = _paczkomatDbContext.Addresses.OrderBy(a => a.Id).LastOrDefault();

                    if (addressRecord is null)
                        newAddressId = 1;
                    else
                        newAddressId = addressRecord.Id + 1;

                    var newAddress = new Address
                    {
                        Id = newAddressId,
                        Country = newUserDto.Country,
                        Province = newUserDto.Province,
                        Town = newUserDto.Town,
                        PostalCode = newUserDto.PostalCode,
                        Street = newUserDto.Street,
                        AddressNumber = (short)newUserDto.AddressNumber
                    };

                    _paczkomatDbContext.Add(newAddress);
                    _paczkomatDbContext.SaveChanges();
                    newUserDto.AddressId = newAddressId;
                }
            }
            else if (newUserDto.Country != null || newUserDto.Province != null || newUserDto.Town != null || newUserDto.PostalCode != null || newUserDto.Street != null || newUserDto.AddressNumber != null)
                    return BadRequest("Nie wypełniono wszystkich pól adresu!");

       

            var possiblyExistingUser = _paczkomatDbContext.Users.Where(u => u.PhoneNumber == newUserDto.PhoneNumber).FirstOrDefault();

            if (possiblyExistingUser is not null)
                return Conflict("Użytkownik o tym numerze telefonu już istnieje!");

            //Mapowanie z forsowaniem typu konta "user"
            var user = _mapper.Map<User>(newUserDto, opt => opt.Items["AccountType"] = "user");
            var hashedPassword = _passwordHasher.HashPassword(user, newUserDto.Password);
            user.Password = hashedPassword;
            _paczkomatDbContext.Add(user);
            _paczkomatDbContext.SaveChanges();

            return Created($"/api/paczkomat/user/{newUserDto.PhoneNumber}", "Dodano użytkownika.");
        }

        
        // generatory haseł
        
        [HttpGet]
        [Route("user/{phoneNumber}/generate/{password}")]
        public ActionResult GetHash([FromRoute] string password, int phoneNumber)
        {
            CreateUserDto newUserDto = new()
            {
                PhoneNumber = phoneNumber,
                Email = "twojstary@gmail.com",
                Name = "bała",
                Surname = "łajka",
                Password = password
            };

            var user = _mapper.Map<User>(newUserDto, opt => opt.Items["AccountType"] = "user");

            if (user is null)
                return NotFound("chuj ci dupe");

            var hashedPassword = _passwordHasher.HashPassword(user, password);

            return Ok(hashedPassword);
        }

        [HttpGet]
        [Route("machine/generate/{password}")]
        public ActionResult GetHashMachine([FromRoute] string password)
        {
            var machine = new Models.Machine
            {
                Id = "pdkp2",
                Description = "NIC",
                Status = "TAKISE",
                Coordinates = "asdas",
                Password = password
            };

            var hashedPassword = _passwordHasherMachine.HashPassword(machine, password);

            return Ok(hashedPassword);
        }
    }
}
