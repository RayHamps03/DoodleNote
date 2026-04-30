using DoodleNote.Models;
using Xunit;

namespace DoodleNote.Tests;

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdIsNotNullOrEmpty()
    {
        ErrorViewModel model = new ErrorViewModel { RequestId = "abc" };
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNullOrEmpty()
    {
        ErrorViewModel model1 = new ErrorViewModel { RequestId = null };
        ErrorViewModel model2 = new ErrorViewModel { RequestId = string.Empty };
        Assert.False(model1.ShowRequestId);
        Assert.False(model2.ShowRequestId);
    }
}