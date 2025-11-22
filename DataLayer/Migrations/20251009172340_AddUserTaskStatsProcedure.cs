using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTaskStatsProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL-код для створення функції (збереженої процедури) в PostgreSQL.
            // Ця функція приймає ID користувача і повертає таблицю з двох колонок:
            // TotalTasks - загальна кількість завдань.
            // OverdueTasks - кількість завдань, у яких дедлайн EndedAt вже минув.
            var sql = @"
CREATE OR REPLACE FUNCTION GetUserTaskStats(p_user_id TEXT)
RETURNS TABLE(TotalTasks BIGINT, OverdueTasks BIGINT) AS $$
BEGIN
    RETURN QUERY
    SELECT
        COUNT(*) AS TotalTasks,
        COUNT(*) FILTER (WHERE ""EndedAt"" < CURRENT_TIMESTAMP) AS OverdueTasks
    FROM ""Items""
    WHERE ""UserGuid"" = p_user_id;
END;
$$ LANGUAGE plpgsql;";

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // SQL-код для видалення функції, якщо міграція буде скасована.
            var sql = @"DROP FUNCTION IF EXISTS GetUserTaskStats(TEXT);";
            migrationBuilder.Sql(sql);
        }
    }
}