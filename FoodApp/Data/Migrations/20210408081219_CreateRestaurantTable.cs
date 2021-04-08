using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodApp.Data.Migrations
{
    public partial class CreateRestaurantTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Dish",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RestaurantId1",
                table: "Dish",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AverageScore",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Dish_RestaurantId1",
                table: "Dish",
                column: "RestaurantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Dish_AspNetUsers_RestaurantId1",
                table: "Dish",
                column: "RestaurantId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dish_AspNetUsers_RestaurantId1",
                table: "Dish");

            migrationBuilder.DropIndex(
                name: "IX_Dish_RestaurantId1",
                table: "Dish");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Dish");

            migrationBuilder.DropColumn(
                name: "RestaurantId1",
                table: "Dish");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AverageScore",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "State",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");
        }
    }
}
