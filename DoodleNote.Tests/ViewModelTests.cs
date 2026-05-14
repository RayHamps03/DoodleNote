using DoodleNote.Features.Admin.Models;
using DoodleNote.Models;
using DoodleNoteModel = DoodleNote.Models.DoodleNote;
using System;
using System.Collections.Generic;
using Xunit;

namespace DoodleNote.Tests;

public class ViewModelTests
{
	[Fact]
	public void PaginatedAccountsViewModel_ComputedFlags_ReflectPageState()
	{
		PaginatedAccountsViewModel model = new()
		{
			CurrentPage = 2,
			TotalPages = 3
		};

		Assert.True(model.HasPreviousPage);
		Assert.True(model.HasNextPage);
	}

	[Fact]
	public void PaginatedAccountsViewModel_Defaults_AreSet()
	{
		PaginatedAccountsViewModel model = new();

		Assert.Equal(1, model.CurrentPage);
		Assert.False(model.HasPreviousPage);
	}

	[Fact]
	public void DoodleNoteDetailsViewModel_Defaults_AreSet()
	{
		DoodleNoteDetailsViewModel model = new();

		Assert.Equal(string.Empty, model.NoteTitle);
		Assert.Equal(string.Empty, model.Author);
		Assert.Equal(string.Empty, model.Description);
	}

	[Fact]
	public void DoodleNoteListViewModel_ComputedFlags_ReflectPageState()
	{
		DoodleNoteListViewModel model = new()
		{
			CurrentPage = 1,
			TotalPages = 1,
			Notes = Array.Empty<DoodleNoteModel>()
		};

		Assert.False(model.HasPreviousPage);
		Assert.False(model.HasNextPage);
	}

	[Fact]
	public void AccountManagementViewModel_InitializesRolesCollection()
	{
		AccountManagementViewModel model = new();

		Assert.NotNull(model.Roles);
		Assert.IsType<List<string>>(model.Roles);
	}
}
