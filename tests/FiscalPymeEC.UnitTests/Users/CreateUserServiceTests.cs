using FiscalPymeEC.Application.Common.Interfaces;
using FiscalPymeEC.Application.Users;
using FiscalPymeEC.Domain.Common;
using FiscalPymeEC.Domain.Users;

namespace FiscalPymeEC.UnitTests.Users;

public sealed class CreateUserServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidData_CreatesSeller()
    {
        var repository = new FakeUserRepository(
            emailExists: false);

        var passwordHasher = new FakePasswordHasher();

        var service = new CreateUserService(
            repository,
            passwordHasher);

        var command = new CreateUserCommand(
            "Vendedor Uno",
            "VENDEDOR@FISCALPYME.EC",
            "Venta123!");

        var result = await service.CreateAsync(command);

        Assert.NotNull(repository.AddedUser);
        Assert.True(repository.SaveChangesWasCalled);
        Assert.True(passwordHasher.HashWasCalled);

        Assert.Equal(
            "Venta123!",
            passwordHasher.ReceivedPassword);

        Assert.Equal(
            "generated-password-hash",
            repository.AddedUser.PasswordHash);

        Assert.Equal(
            UserRole.Seller,
            repository.AddedUser.Role);

        Assert.Equal(
            "vendedor@fiscalpyme.ec",
            repository.AddedUser.Email);

        Assert.Equal(
            repository.AddedUser.Id,
            result.UserId);

        Assert.Equal("Seller", result.Role);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateAsync_WhenEmailAlreadyExists_ThrowsDomainException()
    {
        var repository = new FakeUserRepository(
            emailExists: true);

        var passwordHasher = new FakePasswordHasher();

        var service = new CreateUserService(
            repository,
            passwordHasher);

        var command = new CreateUserCommand(
            "Vendedor Existente",
            "existente@fiscalpyme.ec",
            "Venta123!");

        var exception =
            await Assert.ThrowsAsync<DomainException>(
                () => service.CreateAsync(command));

        Assert.Equal(
            "Ya existe un usuario con ese correo electrónico.",
            exception.Message);

        Assert.Null(repository.AddedUser);
        Assert.False(repository.SaveChangesWasCalled);
        Assert.False(passwordHasher.HashWasCalled);
    }

    [Theory]
    [InlineData("corta")]
    [InlineData("solominusculas1!")]
    [InlineData("SOLOMAYUSCULAS1!")]
    [InlineData("SinNumeros!")]
    [InlineData("SinEspecial123")]
    public async Task CreateAsync_WithInvalidPassword_ThrowsDomainException(
        string password)
    {
        var repository = new FakeUserRepository(
            emailExists: false);

        var passwordHasher = new FakePasswordHasher();

        var service = new CreateUserService(
            repository,
            passwordHasher);

        var command = new CreateUserCommand(
            "Vendedor",
            "vendedor@fiscalpyme.ec",
            password);

        await Assert.ThrowsAsync<DomainException>(
            () => service.CreateAsync(command));

        Assert.Null(repository.AddedUser);
        Assert.False(repository.SaveChangesWasCalled);
        Assert.False(passwordHasher.HashWasCalled);
    }

    private sealed class FakeUserRepository(
        bool emailExists) : IUserRepository
    {
        public User? AddedUser { get; private set; }

        public bool SaveChangesWasCalled { get; private set; }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(emailExists);
        }

        public void Add(User user)
        {
            AddedUser = user;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesWasCalled = true;

            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public bool HashWasCalled { get; private set; }

        public string? ReceivedPassword { get; private set; }

        public string Hash(string password)
        {
            HashWasCalled = true;
            ReceivedPassword = password;

            return "generated-password-hash";
        }

        public bool Verify(
            string password,
            string passwordHash)
        {
            throw new NotSupportedException();
        }
    }
}