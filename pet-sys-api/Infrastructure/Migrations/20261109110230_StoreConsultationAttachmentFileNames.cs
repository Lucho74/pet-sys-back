using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreConsultationAttachmentFileNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AttachmentBlobNames",
                table: "Consultations",
                newName: "Attachments");

            migrationBuilder.Sql("""
                UPDATE consultation
                SET [Attachments] = COALESCE((
                    SELECT attachment.[value] AS [blobName], CAST(NULL AS nvarchar(max)) AS [fileName]
                    FROM OPENJSON(consultation.[Attachments]) AS attachment
                    ORDER BY CONVERT(int, attachment.[key])
                    FOR JSON PATH, INCLUDE_NULL_VALUES
                ), N'[]')
                FROM [Consultations] AS consultation;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE consultation
                SET [Attachments] = N'[' + COALESCE((
                    SELECT STRING_AGG(CAST(N'"' + STRING_ESCAPE(JSON_VALUE(attachment.[value], '$.blobName'), 'json') + N'"' AS nvarchar(max)), N',')
                        WITHIN GROUP (ORDER BY CONVERT(int, attachment.[key]))
                    FROM OPENJSON(consultation.[Attachments]) AS attachment
                ), N'') + N']'
                FROM [Consultations] AS consultation;
                """);

            migrationBuilder.RenameColumn(
                name: "Attachments",
                table: "Consultations",
                newName: "AttachmentBlobNames");
        }
    }
}
