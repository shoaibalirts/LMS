using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using LMS_API.Models;

public class TaskModelTest
{
    // Model validation test to ensure that both PictureUrl and PictureArray cannot be provided at the same time, 
    // and that at least one of them is provided. This test will validate the Task model's data annotations.
    [Theory]
    [MemberData(nameof(TasksWithPictures))]
    public void TaskPictureValidation_ReturnOk(LMS_API.Models.Task task)
    {
        // Arrange
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(task, null, null);

        // Act: trigger model validation
        Validator.TryValidateObject(task, context, validationResults, validateAllProperties: true);

        // Assert based on scenario
        if (task.PictureUrl != null && task.PictureArray != null)
        {
            Assert.True(validationResults.Count > 0, 
                "No validation both Url and array provided");
        }
        else if (task.PictureUrl == null && task.PictureArray == null)
        {
            Assert.True(validationResults.Count > 0,
                "No validation none (url or array) are provided");
        }
        else
        {
            Assert.Empty(validationResults);
        }
    }

    public static IEnumerable<object[]> TasksWithPictures => new List<object[]>
    {
        // Valid: only Url
        new object[] { new LMS_API.Models.Task { PictureUrl = "https://example.com/img1.png", PictureArray = null } },

        // Valid: only Array
        new object[] { new LMS_API.Models.Task { PictureUrl = null, PictureArray = new byte[] { 0xFF, 0xD8 } } },

        // Invalid: both provided
        new object[] { new LMS_API.Models.Task { PictureUrl = "https://example.com/img2.png", PictureArray = new byte[] { 0xFF } } },

        // Invalid: neither provided
        new object[] { new LMS_API.Models.Task { PictureUrl = null, PictureArray = null } },
    };
}