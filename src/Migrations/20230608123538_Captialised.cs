using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace realworld.Migrations
{
    /// <inheritdoc />
    public partial class Captialised : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Users_authorID",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Articles_articleID",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_authorID",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "image",
                table: "Users",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "bio",
                table: "Users",
                newName: "Bio");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Tags",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "Comments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Comments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "Comments",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "authorID",
                table: "Comments",
                newName: "AuthorID");

            migrationBuilder.RenameColumn(
                name: "articleID",
                table: "Comments",
                newName: "ArticleID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_authorID",
                table: "Comments",
                newName: "IX_Comments_AuthorID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_articleID",
                table: "Comments",
                newName: "IX_Comments_ArticleID");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "Articles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "Articles",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "slug",
                table: "Articles",
                newName: "Slug");

            migrationBuilder.RenameColumn(
                name: "favoritesCount",
                table: "Articles",
                newName: "FavoritesCount");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Articles",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Articles",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "Articles",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "authorID",
                table: "Articles",
                newName: "AuthorID");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_authorID",
                table: "Articles",
                newName: "IX_Articles_AuthorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Users_AuthorID",
                table: "Articles",
                column: "AuthorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Articles_ArticleID",
                table: "Comments",
                column: "ArticleID",
                principalTable: "Articles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_AuthorID",
                table: "Comments",
                column: "AuthorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Users_AuthorID",
                table: "Articles");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Articles_ArticleID",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_AuthorID",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Users",
                newName: "image");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Bio",
                table: "Users",
                newName: "bio");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tags",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Comments",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Comments",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Comments",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "AuthorID",
                table: "Comments",
                newName: "authorID");

            migrationBuilder.RenameColumn(
                name: "ArticleID",
                table: "Comments",
                newName: "articleID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_AuthorID",
                table: "Comments",
                newName: "IX_Comments_authorID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ArticleID",
                table: "Comments",
                newName: "IX_Comments_articleID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Articles",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Articles",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Slug",
                table: "Articles",
                newName: "slug");

            migrationBuilder.RenameColumn(
                name: "FavoritesCount",
                table: "Articles",
                newName: "favoritesCount");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Articles",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Articles",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "Articles",
                newName: "body");

            migrationBuilder.RenameColumn(
                name: "AuthorID",
                table: "Articles",
                newName: "authorID");

            migrationBuilder.RenameIndex(
                name: "IX_Articles_AuthorID",
                table: "Articles",
                newName: "IX_Articles_authorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Users_authorID",
                table: "Articles",
                column: "authorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Articles_articleID",
                table: "Comments",
                column: "articleID",
                principalTable: "Articles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_authorID",
                table: "Comments",
                column: "authorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
