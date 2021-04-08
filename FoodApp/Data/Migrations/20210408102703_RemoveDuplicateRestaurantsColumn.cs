using Microsoft.EntityFrameworkCore.Migrations;

namespace FoodApp.Data.Migrations
{
    public partial class RemoveDuplicateRestaurantsColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dish_AspNetUsers_RestaurantId1",
                table: "Dish");

            migrationBuilder.DropIndex(
                name: "IX_Dish_RestaurantId1",
                table: "Dish");

            migrationBuilder.DropColumn(
                name: "RestaurantId1",
                table: "Dish");

            migrationBuilder.AlterColumn<string>(
                name: "RestaurantId",
                table: "Dish",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Dish_RestaurantId",
                table: "Dish",
                column: "RestaurantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dish_AspNetUsers_RestaurantId",
                table: "Dish",
                column: "RestaurantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dish_AspNetUsers_RestaurantId",
                table: "Dish");

            migrationBuilder.DropIndex(
                name: "IX_Dish_RestaurantId",
                table: "Dish");

            migrationBuilder.AlterColumn<int>(
                name: "RestaurantId",
                table: "Dish",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestaurantId1",
                table: "Dish",
                type: "nvarchar(450)",
                nullable: true);

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
    }
}
