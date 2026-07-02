namespace HabitTracker.Application.Auth.Exceptions;

public sealed class EmailAlreadyExistsException(string email)
    : Exception($"Email '{email}' is already registered.");
