using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.Application.Users;

public sealed class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<CreateUserResult> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);

        var emailExists =
            await _userRepository.ExistsByEmailAsync(
                command.Email,
                cancellationToken);

        if (emailExists)
        {
            throw new DomainException(
                "Ya existe un usuario con ese correo electrónico.");
        }

        var passwordHash =
            _passwordHasher.Hash(command.Password);

        var user = new User(
            command.FullName,
            command.Email,
            passwordHash,
            UserRole.Seller);

        _userRepository.Add(user);

        await _userRepository.SaveChangesAsync(
            cancellationToken);

        return new CreateUserResult(
            user.Id,
            user.FullName,
            user.Email,
            user.Role.ToString(),
            user.IsActive);
    }

    private static void ValidateCommand(
        CreateUserCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            throw new DomainException(
                "El nombre completo es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new DomainException(
                "El correo electrónico es obligatorio.");
        }

        ValidatePassword(command.Password);
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new DomainException(
                "La contraseña es obligatoria.");
        }

        if (password.Length < 8)
        {
            throw new DomainException(
                "La contraseña debe tener al menos 8 caracteres.");
        }

        if (!password.Any(char.IsUpper))
        {
            throw new DomainException(
                "La contraseña debe incluir una letra mayúscula.");
        }

        if (!password.Any(char.IsLower))
        {
            throw new DomainException(
                "La contraseña debe incluir una letra minúscula.");
        }

        if (!password.Any(char.IsDigit))
        {
            throw new DomainException(
                "La contraseña debe incluir un número.");
        }

        if (!password.Any(character =>
                !char.IsLetterOrDigit(character)))
        {
            throw new DomainException(
                "La contraseña debe incluir un carácter especial.");
        }
    }
}