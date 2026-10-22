using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Persistence.Context;
using Tillegal.Persistence.DataModels;

namespace Tillegal.Persistence.SeedData;

public sealed class ComprehensiveDatabaseDataSeed(
    AppDbContext dbContext,
    IPasswordHashingService? passwordHashingService = null) : IDevelopmentDataSeed
{
    // Deterministic IDs for idempotency
    public static class SeedIds
    {
        // Roles
        public static readonly Guid RoleAdmin = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public static readonly Guid RoleUser = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public static readonly Guid RoleLawer = Guid.Parse("10000000-0000-0000-0000-000000000003");

        // Users (10 users)
        public static readonly Guid UserAdmin1 = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public static readonly Guid UserAdmin2 = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public static readonly Guid UserRegular1 = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public static readonly Guid UserRegular2 = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public static readonly Guid UserRegular3 = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public static readonly Guid UserRegular4 = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public static readonly Guid UserRegular5 = Guid.Parse("20000000-0000-0000-0000-000000000007");
        public static readonly Guid UserLawer1 = Guid.Parse("20000000-0000-0000-0000-000000000008");
        public static readonly Guid UserLawer2 = Guid.Parse("20000000-0000-0000-0000-000000000009");
        public static readonly Guid UserLawer3 = Guid.Parse("20000000-0000-0000-0000-000000000010");
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedUsersAsync(cancellationToken);
        await SeedConversationsAndMessagesAsync(cancellationToken);
        await SeedUserAttachmentsAsync(cancellationToken);
        await SeedAiEvaluationLogsAsync(cancellationToken);
        await SeedAuditLogsAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var roles = new List<RoleDataModel>
        {
            new() { Id = SeedIds.RoleAdmin, Name = "Admin" },
            new() { Id = SeedIds.RoleUser, Name = "User" },
            new() { Id = SeedIds.RoleLawer, Name = "Lawer" }
        };

        var existingRoleIds = await dbContext.Roles
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        var missingRoles = roles.Where(r => !existingRoleIds.Contains(r.Id)).ToList();
        if (missingRoles.Count > 0)
        {
            await dbContext.Roles.AddRangeAsync(missingRoles, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        var passwordHash = passwordHashingService?.HashPassword("P@ssword123!")
            ?? "AQAAAAIAAYagAAAAEG3g1R6U7lS6s5n8+f2R7vM+SampleHashedPasswordFallback=";

        var baseTime = DateTimeOffset.UtcNow.AddMonths(-3);

        var users = new List<UserDataModel>
        {
            // 2 Admin Accounts
            new()
            {
                Id = SeedIds.UserAdmin1,
                RoleId = SeedIds.RoleAdmin,
                FullName = "Quản Trị Viên Hệ Thống",
                UserName = "admin_system",
                Email = "admin.system@tillegal.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0901000001",
                CreatedAt = baseTime
            },
            new()
            {
                Id = SeedIds.UserAdmin2,
                RoleId = SeedIds.RoleAdmin,
                FullName = "Giám Sát An Toàn Thông Tin",
                UserName = "admin_security",
                Email = "admin.security@tillegal.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0901000002",
                CreatedAt = baseTime.AddDays(2)
            },

            // 5 Regular Users with varying active / modified states
            new()
            {
                Id = SeedIds.UserRegular1,
                RoleId = SeedIds.RoleUser,
                FullName = "Nguyễn Văn A",
                UserName = "nguyen_vana",
                Email = "nguyen.vana@gmail.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0912000001",
                CreatedAt = baseTime.AddDays(5)
            },
            new()
            {
                Id = SeedIds.UserRegular2,
                RoleId = SeedIds.RoleUser,
                FullName = "Trần Thị Bích Ngọc",
                UserName = "tran_bichngoc",
                Email = "tran.bichngoc@gmail.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0912000002",
                CreatedAt = baseTime.AddDays(7),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10) // Updated info
            },
            new()
            {
                Id = SeedIds.UserRegular3,
                RoleId = SeedIds.RoleUser,
                FullName = "Lê Hoàng Long",
                UserName = "le_long",
                Email = "le.hoanglong@yahoo.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0912000003",
                CreatedAt = baseTime.AddDays(10)
            },
            new()
            {
                Id = SeedIds.UserRegular4,
                RoleId = SeedIds.RoleUser,
                FullName = "Phạm Thị Thùy Dung",
                UserName = "pham_thuydung",
                Email = "pham.thuydung@outlook.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0912000004",
                CreatedAt = baseTime.AddDays(15),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5) // Updated info
            },
            new()
            {
                Id = SeedIds.UserRegular5,
                RoleId = SeedIds.RoleUser,
                FullName = "Vũ Đình Trọng",
                UserName = "vu_dinhtrong",
                Email = "vu.dinhtrong@gmail.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0912000005",
                CreatedAt = baseTime.AddDays(20)
            },

            // 3 Lawer Users with varying active / modified states
            new()
            {
                Id = SeedIds.UserLawer1,
                RoleId = SeedIds.RoleLawer,
                FullName = "Luật Sư Trần Quang Minh",
                UserName = "lawer_minh",
                Email = "luatsu.minh@tillegal.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0988000001",
                CreatedAt = baseTime.AddDays(1)
            },
            new()
            {
                Id = SeedIds.UserLawer2,
                RoleId = SeedIds.RoleLawer,
                FullName = "Luật Sư Đặng Thu Hà",
                UserName = "lawer_thuha",
                Email = "luatsu.thuha@tillegal.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0988000002",
                CreatedAt = baseTime.AddDays(3),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-2) // Updated profile
            },
            new()
            {
                Id = SeedIds.UserLawer3,
                RoleId = SeedIds.RoleLawer,
                FullName = "Luật Sư Nguyễn Quốc Anh",
                UserName = "lawer_quocanh",
                Email = "luatsu.quocanh@tillegal.com",
                PasswordHash = passwordHash,
                PhoneNumber = "0988000003",
                CreatedAt = baseTime.AddDays(4)
            }
        };

        var existingUserIds = await dbContext.Users
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var missingUsers = users.Where(u => !existingUserIds.Contains(u.Id)).ToList();
        if (missingUsers.Count > 0)
        {
            await dbContext.Users.AddRangeAsync(missingUsers, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedConversationsAndMessagesAsync(CancellationToken cancellationToken)
    {
        // 15 Conversations
        var conversationSpecs = new (Guid Id, Guid UserId, string Title, string TopicArea)[]
        {
            (Guid.Parse("30000000-0000-0000-0000-000000000001"), SeedIds.UserRegular1, "Tư vấn thủ tục thành lập công ty TNHH 2 thành viên", "Doanh nghiệp"),
            (Guid.Parse("30000000-0000-0000-0000-000000000002"), SeedIds.UserRegular1, "Quy định về thời hạn góp vốn điều lệ và chế tài vi phạm", "Doanh nghiệp"),
            (Guid.Parse("30000000-0000-0000-0000-000000000003"), SeedIds.UserRegular2, "Tranh chấp hợp đồng mua bán hàng hóa và bồi thường thiệt hại", "Hợp đồng"),
            (Guid.Parse("30000000-0000-0000-0000-000000000004"), SeedIds.UserRegular2, "Đơn phương chấm dứt hợp đồng lao động và trợ cấp thôi việc", "Lao động"),
            (Guid.Parse("30000000-0000-0000-0000-000000000005"), SeedIds.UserRegular3, "Thủ tục đăng ký bảo hộ nhãn hiệu và bản quyền tác giả", "Sở hữu trí tuệ"),
            (Guid.Parse("30000000-0000-0000-0000-000000000006"), SeedIds.UserRegular3, "Nghĩa vụ thuế thu nhập doanh nghiệp đối với startup đổi mới sáng tạo", "Thuế"),
            (Guid.Parse("30000000-0000-0000-0000-000000000007"), SeedIds.UserRegular4, "Điều kiện phát hành trái phiếu riêng lẻ của công ty cổ phần", "Tài chính"),
            (Guid.Parse("30000000-0000-0000-0000-000000000008"), SeedIds.UserRegular4, "Thủ tục chia thừa kế nhà đất theo pháp luật có yếu tố nước ngoài", "Dân sự"),
            (Guid.Parse("30000000-0000-0000-0000-000000000009"), SeedIds.UserRegular5, "Thẩm quyền giải quyết tranh chấp đất đai theo Luật Đất đai mới", "Đất đai"),
            (Guid.Parse("30000000-0000-0000-0000-000000000010"), SeedIds.UserLawer1, "Soạn thảo thỏa thuận bảo mật thông tin (NDA) và cam kết không cạnh tranh", "Hợp đồng"),
            (Guid.Parse("30000000-0000-0000-0000-000000000011"), SeedIds.UserLawer1, "Quy trình thẩm định pháp lý (Legal Due Diligence) trong thương vụ M&A", "M&A"),
            (Guid.Parse("30000000-0000-0000-0000-000000000012"), SeedIds.UserLawer2, "Xử lý vi phạm nghĩa vụ thanh toán trong hợp đồng thương mại điện tử", "Thương mại"),
            (Guid.Parse("30000000-0000-0000-0000-000000000013"), SeedIds.UserLawer2, "Tuân thủ quy định bảo vệ dữ liệu cá nhân theo Nghị định 13/2023/NĐ-CP", "Công nghệ"),
            (Guid.Parse("30000000-0000-0000-0000-000000000014"), SeedIds.UserLawer3, "Thủ tục xin cấp giấy phép lao động cho người lao động nước ngoài", "Lao động"),
            (Guid.Parse("30000000-0000-0000-0000-000000000015"), SeedIds.UserLawer3, "Trình tự giải thể doanh nghiệp và thứ tự ưu tiên thanh toán nghĩa vụ nợ", "Doanh nghiệp")
        };

        var existingConvIds = await dbContext.Conversations
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var conversationsToAdd = new List<ConversationDataModel>();
        foreach (var spec in conversationSpecs)
        {
            if (!existingConvIds.Contains(spec.Id))
            {
                conversationsToAdd.Add(new ConversationDataModel
                {
                    Id = spec.Id,
                    UserId = spec.UserId,
                    Title = spec.Title,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-15)
                });
            }
        }

        if (conversationsToAdd.Count > 0)
        {
            await dbContext.Conversations.AddRangeAsync(conversationsToAdd, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        // 60 Conversation Messages (4 messages per conversation: 2 user + 2 assistant)
        var existingMessageIds = await dbContext.ConversationMessages
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        var messagesToAdd = new List<ConversationMessageDataModel>();

        for (var i = 0; i < conversationSpecs.Length; i++)
        {
            var spec = conversationSpecs[i];
            var convIdx = i + 1;

            var msgId1 = Guid.Parse($"40000000-0000-0000-0000-{convIdx:D4}00000001");
            var msgId2 = Guid.Parse($"40000000-0000-0000-0000-{convIdx:D4}00000002");
            var msgId3 = Guid.Parse($"40000000-0000-0000-0000-{convIdx:D4}00000003");
            var msgId4 = Guid.Parse($"40000000-0000-0000-0000-{convIdx:D4}00000004");

            // Msg 1: User question
            if (!existingMessageIds.Contains(msgId1))
            {
                messagesToAdd.Add(new ConversationMessageDataModel
                {
                    Id = msgId1,
                    ConversationId = spec.Id,
                    Role = "user",
                    Content = $"Xin chào luật sư, tôi muốn tìm hiểu các căn cứ pháp lý và trình tự thủ tục liên quan đến: {spec.Title}?",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-14).AddHours(convIdx)
                });
            }

            // Msg 2: Assistant detailed response with superscript index and JSON citations
            if (!existingMessageIds.Contains(msgId2))
            {
                var (content, citationsJson) = BuildAssistantLegalResponse(spec.TopicArea, 1);
                messagesToAdd.Add(new ConversationMessageDataModel
                {
                    Id = msgId2,
                    ConversationId = spec.Id,
                    Role = "assistant",
                    Content = content,
                    Citations = JsonDocument.Parse(citationsJson),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-14).AddHours(convIdx).AddMinutes(1)
                });
            }

            // Msg 3: User follow-up question
            if (!existingMessageIds.Contains(msgId3))
            {
                messagesToAdd.Add(new ConversationMessageDataModel
                {
                    Id = msgId3,
                    ConversationId = spec.Id,
                    Role = "user",
                    Content = "Trường hợp có phát sinh tranh chấp hoặc chậm trễ thì thời hiệu xử lý và mức phạt được quy định như thế nào?",
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-13).AddHours(convIdx)
                });
            }

            // Msg 4: Assistant follow-up response with citations
            if (!existingMessageIds.Contains(msgId4))
            {
                var (content, citationsJson) = BuildAssistantLegalResponse(spec.TopicArea, 2);
                messagesToAdd.Add(new ConversationMessageDataModel
                {
                    Id = msgId4,
                    ConversationId = spec.Id,
                    Role = "assistant",
                    Content = content,
                    Citations = JsonDocument.Parse(citationsJson),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-13).AddHours(convIdx).AddMinutes(2)
                });
            }
        }

        if (messagesToAdd.Count > 0)
        {
            await dbContext.ConversationMessages.AddRangeAsync(messagesToAdd, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static (string Content, string CitationsJson) BuildAssistantLegalResponse(string topicArea, int round)
    {
        if (round == 1)
        {
            var content = """
                ### 1. Cơ sở pháp lý điều chỉnh
                Theo quy định hiện hành tại hệ thống pháp luật Việt Nam[^1], việc thực hiện các thủ tục phải tuân thủ nghiêm ngặt hồ sơ, thành phần và thời hạn luật định. Cụ thể:

                - **Hồ sơ đăng ký/yêu cầu**: Phải lập thành văn bản theo mẫu chuẩn do cơ quan có thẩm quyền ban hành[^2].
                - **Thời gian xử lý**: Trong thời hạn từ 03 đến 15 ngày làm việc kể từ ngày nhận đủ hồ sơ hợp lệ.

                ### 2. Các điểm trọng yếu cần lưu ý
                > **Lưu ý quan trọng**: Doanh nghiệp và cá nhân cần chủ động rà soát tư cách pháp lý của các bên tham gia nhằm hạn chế tối đa rủi ro vô hiệu giao dịch.

                1. Đảm bảo tính trung thực, chính xác của thông tin kê khai.
                2. Nộp đầy đủ các khoản phí, lệ phí nhà nước theo quy định.
                """;

            var citations = new[]
            {
                new
                {
                    id = "cit-001",
                    index = 1,
                    documentTitle = "Luật Doanh nghiệp 2020",
                    documentNumber = "59/2020/QH14",
                    articleReference = "Điều 23, Khoản 1",
                    sourceUrl = "https://thuvienphapluat.vn/van-ban/Doanh-nghiep/Luat-Doanh-nghiep-2020-447545.aspx",
                    citedText = "Hồ sơ đăng ký doanh nghiệp phải bao gồm giấy đề nghị đăng ký doanh nghiệp và danh sách thành viên/cổ đông sáng lập."
                },
                new
                {
                    id = "cit-002",
                    index = 2,
                    documentTitle = "Nghị định 01/2021/NĐ-CP về đăng ký doanh nghiệp",
                    documentNumber = "01/2021/NĐ-CP",
                    articleReference = "Điều 32, Khoản 2",
                    sourceUrl = "https://thuvienphapluat.vn/van-ban/Doanh-nghiep/Nghi-dinh-01-2021-ND-CP-dang-ky-doanh-nghiep-462002.aspx",
                    citedText = "Cơ quan đăng ký kinh doanh có trách nhiệm xem xét tính hợp lệ của hồ sơ và cấp Giấy chứng nhận đăng ký doanh nghiệp trong thời hạn 03 ngày làm việc."
                }
            };

            return (content, JsonSerializer.Serialize(citations));
        }
        else
        {
            var content = """
                ### 1. Về chế tài và trách nhiệm pháp lý
                Trường hợp vi phạm thời hạn hoặc nghĩa vụ đã cam kết, cá nhân/tổ chức vi phạm có thể bị áp dụng các biện pháp xử phạt vi phạm hành chính[^1], đồng thời phải bồi thường thiệt hại thực tế phát sinh nếu có lỗi[^2].

                ### 2. Thời hiệu yêu cầu giải quyết
                - **Thời hiệu khởi kiện tranh chấp**: Thường là **02 năm đến 03 năm** kể từ ngày người có quyền lợi biết hoặc phải biết quyền lợi của mình bị xâm phạm.
                - **Phương thức xử lý**: Khuyến khích thương lượng, hòa giải trước khi đưa vụ việc ra Trọng tài thương mại hoặc Tòa án nhân dân có thẩm quyền.
                """;

            var citations = new[]
            {
                new
                {
                    id = "cit-003",
                    index = 1,
                    documentTitle = "Nghị định 122/2021/NĐ-CP xử phạt vi phạm hành chính lĩnh vực kế hoạch và đầu tư",
                    documentNumber = "122/2021/NĐ-CP",
                    articleReference = "Điều 46, Khoản 3",
                    sourceUrl = "https://thuvienphapluat.vn/van-ban/Vi-pham-hanh-chinh/Nghi-dinh-122-2021-ND-CP-xu-phat-vi-pham-hanh-chinh-ke-hoach-dau-tu-498902.aspx",
                    citedText = "Phạt tiền từ 20.000.000 đồng đến 30.000.000 đồng đối với hành vi không đăng ký thay đổi nội dung Giấy chứng nhận đăng ký doanh nghiệp."
                },
                new
                {
                    id = "cit-004",
                    index = 2,
                    documentTitle = "Bộ luật Dân sự 2015",
                    documentNumber = "91/2015/QH13",
                    articleReference = "Điều 360",
                    sourceUrl = "https://thuvienphapluat.vn/van-ban/Bat-dong-san/Bo-luat-dan-su-2015-279332.aspx",
                    citedText = "Trường hợp có thiệt hại do vi phạm nghĩa vụ gây ra thì bên có nghĩa vụ phải bồi thường toàn bộ thiệt hại, trừ trường hợp có thỏa thuận khác hoặc luật có quy định khác."
                }
            };

            return (content, JsonSerializer.Serialize(citations));
        }
    }

    private async Task SeedUserAttachmentsAsync(CancellationToken cancellationToken)
    {
        // 8 User Attachments with diverse ProcessingStatus: Pending, Processing, Completed, Failed
        var attachments = new List<UserAttachmentDataModel>
        {
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                UserId = SeedIds.UserRegular1,
                FileName = "Dieu_le_cong_ty_TNHH_mau.pdf",
                StoragePath = "attachments/user-1/dieu_le_cong_ty_mau.pdf",
                FileSize = 1450210,
                ContentType = "application/pdf",
                ProcessingStatus = "Completed",
                TokenCount = 2450,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-12)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                UserId = SeedIds.UserRegular2,
                FileName = "Hop_dong_mua_ban_hang_hoa_quoc_te.docx",
                StoragePath = "attachments/user-2/hop_dong_mua_ban.docx",
                FileSize = 820400,
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ProcessingStatus = "Completed",
                TokenCount = 1820,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-11)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                UserId = SeedIds.UserRegular2,
                FileName = "Hop_dong_lao_dong_2024.pdf",
                StoragePath = "attachments/user-2/hop_dong_lao_dong.pdf",
                FileSize = 512000,
                ContentType = "application/pdf",
                ProcessingStatus = "Processing",
                TokenCount = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000005"),
                UserId = SeedIds.UserRegular3,
                FileName = "To_khai_dang_ky_nhan_hieu.pdf",
                StoragePath = "attachments/user-3/don_nhan_hieu.pdf",
                FileSize = 950300,
                ContentType = "application/pdf",
                ProcessingStatus = "Pending",
                TokenCount = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-9)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000005"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000007"),
                UserId = SeedIds.UserRegular4,
                FileName = "Bao_cao_tai_chinh_kiem_toan_2024.xlsx",
                StoragePath = "attachments/user-4/bctc_kiem_toan.xlsx",
                FileSize = 2340110,
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ProcessingStatus = "Completed",
                TokenCount = 4200,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-8)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000006"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000009"),
                UserId = SeedIds.UserRegular5,
                FileName = "Giay_chung_nhan_quyen_su_dung_dat_scan.pdf",
                StoragePath = "attachments/user-5/so_do_scan.pdf",
                FileSize = 4800200,
                ContentType = "application/pdf",
                ProcessingStatus = "Failed",
                TokenCount = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-7)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000007"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000010"),
                UserId = SeedIds.UserLawer1,
                FileName = "Thoa_thuan_bao_mat_thong_tin_NDA.docx",
                StoragePath = "attachments/lawer-1/nda_agreement.docx",
                FileSize = 640000,
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ProcessingStatus = "Completed",
                TokenCount = 1540,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-6)
            },
            new()
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000008"),
                ConversationId = Guid.Parse("30000000-0000-0000-0000-000000000015"),
                UserId = SeedIds.UserLawer3,
                FileName = "Phuong_an_phuc_hoi_hoat_dong_kinh_doanh.pdf",
                StoragePath = "attachments/lawer-3/phuong_an_kinh_doanh.pdf",
                FileSize = 1890000,
                ContentType = "application/pdf",
                ProcessingStatus = "Pending",
                TokenCount = null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            }
        };

        var existingAttachmentIds = await dbContext.UserAttachments
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var missingAttachments = attachments.Where(a => !existingAttachmentIds.Contains(a.Id)).ToList();
        if (missingAttachments.Count > 0)
        {
            await dbContext.UserAttachments.AddRangeAsync(missingAttachments, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedAiEvaluationLogsAsync(CancellationToken cancellationToken)
    {
        // 15 AiEvaluationLogs attached to assistant messages
        var evaluationSpecs = new (Guid Id, int ConvIndex, bool IsHelpful, int Rating, string Comment)[]
        {
            (Guid.Parse("60000000-0000-0000-0000-000000000001"), 1, true, 5, "Câu trả lời rất rõ ràng, trích dẫn đúng Luật Doanh nghiệp 2020."),
            (Guid.Parse("60000000-0000-0000-0000-000000000002"), 2, true, 5, "Thông tin chuẩn xác, giúp tôi giải quyết ngay vướng mắc về thời hạn góp vốn."),
            (Guid.Parse("60000000-0000-0000-0000-000000000003"), 3, true, 4, "Tư vấn hợp lý, cần bổ sung thêm án lệ mẫu về phạt vi phạm hợp đồng."),
            (Guid.Parse("60000000-0000-0000-0000-000000000004"), 4, true, 5, "Rất chi tiết về quy định tính trợ cấp thôi việc theo Bộ luật Lao động."),
            (Guid.Parse("60000000-0000-0000-0000-000000000005"), 5, true, 4, "Hướng dẫn đầy đủ các bước nộp đơn đăng ký bảo hộ nhãn hiệu."),
            (Guid.Parse("60000000-0000-0000-0000-000000000006"), 6, false, 2, "Chính sách ưu đãi thuế cho công ty công nghệ cao chưa được cập nhật thông tư mới nhất."),
            (Guid.Parse("60000000-0000-0000-0000-000000000007"), 7, true, 5, "Nắm rất vững các điều kiện phát hành trái phiếu theo Nghị định 65."),
            (Guid.Parse("60000000-0000-0000-0000-000000000008"), 8, true, 4, "Phân tích thỏa đáng về thẩm quyền giải quyết di sản thừa kế có người ở nước ngoài."),
            (Guid.Parse("60000000-0000-0000-0000-000000000009"), 9, false, 3, "Cần trích dẫn chi tiết hơn các điểm mới của Luật Đất đai 2024."),
            (Guid.Parse("60000000-0000-0000-0000-000000000010"), 10, true, 5, "Bản dự thảo điều khoản bảo mật thông tin (NDA) rất chuyên nghiệp."),
            (Guid.Parse("60000000-0000-0000-0000-000000000011"), 11, true, 5, "Checklist thẩm định pháp lý M&A rất đầy đủ các rủi ro tuân thủ."),
            (Guid.Parse("60000000-0000-0000-0000-000000000012"), 12, true, 4, "Giải thích thỏa đáng về bằng chứng điện tử trong tranh chấp TMĐT."),
            (Guid.Parse("60000000-0000-0000-0000-000000000013"), 13, true, 5, "Trích dẫn cực kỳ chuẩn xác các nghĩa vụ theo Nghị định 13/2023 về bảo vệ dữ liệu cá nhân."),
            (Guid.Parse("60000000-0000-0000-0000-000000000014"), 14, false, 2, "Hồ sơ giấy phép lao động cần phân biệt rõ giữa chuyên gia và lao động kỹ thuật."),
            (Guid.Parse("60000000-0000-0000-0000-000000000015"), 15, true, 5, "Thứ tự ưu tiên phân chia tài sản khi giải thể doanh nghiệp rất rõ ràng, logic.")
        };

        var existingEvalIds = await dbContext.AiEvaluationLogs
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        var evaluationsToAdd = new List<AiEvaluationLogDataModel>();

        foreach (var spec in evaluationSpecs)
        {
            if (!existingEvalIds.Contains(spec.Id))
            {
                // Attach to Assistant Msg (msgId 2) of that conversation
                var assistantMessageId = Guid.Parse($"40000000-0000-0000-0000-{spec.ConvIndex:D4}00000002");
                var evaluatorUser = spec.ConvIndex switch
                {
                    <= 2 => SeedIds.UserRegular1,
                    <= 4 => SeedIds.UserRegular2,
                    <= 6 => SeedIds.UserRegular3,
                    <= 8 => SeedIds.UserRegular4,
                    9 => SeedIds.UserRegular5,
                    <= 11 => SeedIds.UserLawer1,
                    <= 13 => SeedIds.UserLawer2,
                    _ => SeedIds.UserLawer3
                };

                evaluationsToAdd.Add(new AiEvaluationLogDataModel
                {
                    Id = spec.Id,
                    MessageId = assistantMessageId,
                    EvaluatorUserId = evaluatorUser,
                    IsHelpful = spec.IsHelpful,
                    Rating = spec.Rating,
                    FeedbackComment = spec.Comment,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-14).AddHours(spec.ConvIndex).AddMinutes(10)
                });
            }
        }

        if (evaluationsToAdd.Count > 0)
        {
            await dbContext.AiEvaluationLogs.AddRangeAsync(evaluationsToAdd, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task SeedAuditLogsAsync(CancellationToken cancellationToken)
    {
        // 30 Audit Logs
        var existingAuditIds = await dbContext.AuditLogs
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var auditLogs = new List<AuditLogDataModel>();

        // 1. LOGIN_SUCCESS (10 logs)
        var usersForLogin = new[]
        {
            (SeedIds.UserAdmin1, "Admin", "192.168.1.10"),
            (SeedIds.UserAdmin2, "Admin", "192.168.1.11"),
            (SeedIds.UserRegular1, "User", "14.232.208.45"),
            (SeedIds.UserRegular2, "User", "113.161.72.102"),
            (SeedIds.UserRegular3, "User", "171.244.15.88"),
            (SeedIds.UserRegular4, "User", "27.72.60.15"),
            (SeedIds.UserRegular5, "User", "118.70.188.22"),
            (SeedIds.UserLawer1, "Lawer", "1.53.24.120"),
            (SeedIds.UserLawer2, "Lawer", "115.79.138.90"),
            (SeedIds.UserLawer3, "Lawer", "123.24.205.65")
        };

        for (var i = 0; i < usersForLogin.Length; i++)
        {
            var id = Guid.Parse($"70000000-0000-0000-0000-{i + 1:D12}");
            if (!existingAuditIds.Contains(id))
            {
                var (userId, role, ip) = usersForLogin[i];
                auditLogs.Add(new AuditLogDataModel
                {
                    Id = id,
                    EventType = "AUTH_EVENT",
                    ActorUserId = userId,
                    ActorRole = role,
                    IpAddress = ip,
                    TargetType = "User",
                    TargetUserId = userId,
                    Action = "LOGIN_SUCCESS",
                    Status = "SUCCESS",
                    Description = $"Người dùng đăng nhập thành công vào hệ thống từ địa chỉ IP {ip}.",
                    NewValues = JsonDocument.Parse($"{{\"method\":\"Password\",\"ipAddress\":\"{ip}\"}}"),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-20).AddHours(i)
                });
            }
        }

        // 2. CREATE_CONVERSATION (8 logs)
        var convAuditUsers = new[]
        {
            (SeedIds.UserRegular1, "User", "14.232.208.45", Guid.Parse("30000000-0000-0000-0000-000000000001"), "Tư vấn thủ tục thành lập công ty TNHH 2 thành viên"),
            (SeedIds.UserRegular1, "User", "14.232.208.45", Guid.Parse("30000000-0000-0000-0000-000000000002"), "Quy định về thời hạn góp vốn điều lệ và chế tài vi phạm"),
            (SeedIds.UserRegular2, "User", "113.161.72.102", Guid.Parse("30000000-0000-0000-0000-000000000003"), "Tranh chấp hợp đồng mua bán hàng hóa và bồi thường thiệt hại"),
            (SeedIds.UserRegular2, "User", "113.161.72.102", Guid.Parse("30000000-0000-0000-0000-000000000004"), "Đơn phương chấm dứt hợp đồng lao động và trợ cấp thôi việc"),
            (SeedIds.UserRegular3, "User", "171.244.15.88", Guid.Parse("30000000-0000-0000-0000-000000000005"), "Thủ tục đăng ký bảo hộ nhãn hiệu và bản quyền tác giả"),
            (SeedIds.UserRegular4, "User", "27.72.60.15", Guid.Parse("30000000-0000-0000-0000-000000000007"), "Điều kiện phát hành trái phiếu riêng lẻ của công ty cổ phần"),
            (SeedIds.UserLawer1, "Lawer", "1.53.24.120", Guid.Parse("30000000-0000-0000-0000-000000000010"), "Soạn thảo thỏa thuận bảo mật thông tin (NDA) và cam kết không cạnh tranh"),
            (SeedIds.UserLawer2, "Lawer", "115.79.138.90", Guid.Parse("30000000-0000-0000-0000-000000000013"), "Tuân thủ quy định bảo vệ dữ liệu cá nhân theo Nghị định 13/2023/NĐ-CP")
        };

        for (var i = 0; i < convAuditUsers.Length; i++)
        {
            var id = Guid.Parse($"70000000-0000-0000-0000-{i + 11:D12}");
            if (!existingAuditIds.Contains(id))
            {
                var (userId, role, ip, convId, title) = convAuditUsers[i];
                auditLogs.Add(new AuditLogDataModel
                {
                    Id = id,
                    EventType = "CONVERSATION_EVENT",
                    ActorUserId = userId,
                    ActorRole = role,
                    IpAddress = ip,
                    TargetType = "Conversation",
                    TargetUserId = userId,
                    Action = "CREATE_CONVERSATION",
                    Status = "SUCCESS",
                    Description = $"Khởi tạo phiên hội thoại mới: '{title}'.",
                    NewValues = JsonDocument.Parse($"{{\"conversationId\":\"{convId}\",\"title\":\"{title}\"}}"),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-15).AddHours(i)
                });
            }
        }

        // 3. UPLOAD_ATTACHMENT (6 logs)
        var uploadAudits = new[]
        {
            (SeedIds.UserRegular1, "User", "14.232.208.45", "Dieu_le_cong_ty_TNHH_mau.pdf", 1450210),
            (SeedIds.UserRegular2, "User", "113.161.72.102", "Hop_dong_mua_ban_hang_hoa_quoc_te.docx", 820400),
            (SeedIds.UserRegular2, "User", "113.161.72.102", "Hop_dong_lao_dong_2024.pdf", 512000),
            (SeedIds.UserRegular3, "User", "171.244.15.88", "To_khai_dang_ky_nhan_hieu.pdf", 950300),
            (SeedIds.UserRegular4, "User", "27.72.60.15", "Bao_cao_tai_chinh_kiem_toan_2024.xlsx", 2340110),
            (SeedIds.UserLawer1, "Lawer", "1.53.24.120", "Thoa_thuan_bao_mat_thong_tin_NDA.docx", 640000)
        };

        for (var i = 0; i < uploadAudits.Length; i++)
        {
            var id = Guid.Parse($"70000000-0000-0000-0000-{i + 19:D12}");
            if (!existingAuditIds.Contains(id))
            {
                var (userId, role, ip, fileName, fileSize) = uploadAudits[i];
                auditLogs.Add(new AuditLogDataModel
                {
                    Id = id,
                    EventType = "STORAGE_EVENT",
                    ActorUserId = userId,
                    ActorRole = role,
                    IpAddress = ip,
                    TargetType = "UserAttachment",
                    TargetUserId = userId,
                    Action = "UPLOAD_ATTACHMENT",
                    Status = "SUCCESS",
                    Description = $"Tải lên tài liệu đính kèm '{fileName}' ({fileSize} bytes).",
                    NewValues = JsonDocument.Parse($"{{\"fileName\":\"{fileName}\",\"fileSize\":{fileSize}}}"),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-10).AddHours(i)
                });
            }
        }

        // 4. EVALUATE_MESSAGE (6 logs)
        var evalAudits = new[]
        {
            (SeedIds.UserRegular1, "User", "14.232.208.45", 5, true),
            (SeedIds.UserRegular2, "User", "113.161.72.102", 5, true),
            (SeedIds.UserRegular3, "User", "171.244.15.88", 2, false),
            (SeedIds.UserRegular4, "User", "27.72.60.15", 4, true),
            (SeedIds.UserLawer1, "Lawer", "1.53.24.120", 5, true),
            (SeedIds.UserLawer2, "Lawer", "115.79.138.90", 5, true)
        };

        for (var i = 0; i < evalAudits.Length; i++)
        {
            var id = Guid.Parse($"70000000-0000-0000-0000-{i + 25:D12}");
            if (!existingAuditIds.Contains(id))
            {
                var (userId, role, ip, rating, isHelpful) = evalAudits[i];
                auditLogs.Add(new AuditLogDataModel
                {
                    Id = id,
                    EventType = "AI_FEEDBACK_EVENT",
                    ActorUserId = userId,
                    ActorRole = role,
                    IpAddress = ip,
                    TargetType = "AiEvaluationLog",
                    TargetUserId = userId,
                    Action = "EVALUATE_MESSAGE",
                    Status = "SUCCESS",
                    Description = $"Đánh giá phản hồi trợ lý AI: Rating {rating}/5 sao, Hữu ích={isHelpful}.",
                    NewValues = JsonDocument.Parse($"{{\"rating\":{rating},\"isHelpful\":{isHelpful.ToString().ToLower()}}}"),
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-8).AddHours(i)
                });
            }
        }

        if (auditLogs.Count > 0)
        {
            await dbContext.AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
