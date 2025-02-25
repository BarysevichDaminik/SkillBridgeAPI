using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SkillBridgeAPI.Models;

public partial class SkillbridgeContext : DbContext
{
    public SkillbridgeContext()
    {
    }

    public SkillbridgeContext(DbContextOptions<SkillbridgeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Exchange> Exchanges { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userskill> Userskills { get; set; }

    public virtual DbSet<RefreshToken> RefreshToken { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("exchange_status_enum", new[] { "pending", "active", "completed", "cancelled", "disputed" })
            .HasPostgresEnum("user_status_enum", new[] { "active", "inactive", "pending", "blocked" });

        modelBuilder.Entity<RefreshToken>(entity => {
            entity.HasKey(e => e.TokenId).HasName("token_id");

            entity.ToTable("refresh_token");

            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at").IsRequired();
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("chat_pkey");

            entity.ToTable("chat");

            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.ChatName).HasColumnName("chat_name");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_date");
            entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");

            entity.HasOne(d => d.Exchange).WithMany(p => p.Chats)
                .HasForeignKey(d => d.ExchangeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("chat_exchange_id_fkey");
        });

        modelBuilder.Entity<Exchange>(entity =>
        {
            entity.HasKey(e => e.ExchangeId).HasName("exchange_pkey");

            entity.ToTable("exchange");

            entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.SkillId1).HasColumnName("skill_id_1");
            entity.Property(e => e.SkillId2).HasColumnName("skill_id_2");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UserId1).HasColumnName("user_id_1");
            entity.Property(e => e.UserId2).HasColumnName("user_id_2");

            entity.HasOne(d => d.SkillId1Navigation).WithMany(p => p.ExchangeSkillId1Navigations)
                .HasForeignKey(d => d.SkillId1)
                .HasConstraintName("exchange_skill_id_1_fkey");

            entity.HasOne(d => d.SkillId2Navigation).WithMany(p => p.ExchangeSkillId2Navigations)
                .HasForeignKey(d => d.SkillId2)
                .HasConstraintName("exchange_skill_id_2_fkey");

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.ExchangeUserId1Navigations)
                .HasForeignKey(d => d.UserId1)
                .HasConstraintName("exchange_user_id_1_fkey");

            entity.HasOne(d => d.UserId2Navigation).WithMany(p => p.ExchangeUserId2Navigations)
                .HasForeignKey(d => d.UserId2)
                .HasConstraintName("exchange_user_id_2_fkey");

            entity.HasMany(d => d.ChatsNavigation).WithMany(p => p.Exchanges)
                .UsingEntity<Dictionary<string, object>>(
                    "Sessionchat",
                    r => r.HasOne<Chat>().WithMany()
                        .HasForeignKey("ChatId")
                        .HasConstraintName("sessionchat_chat_id_fkey"),
                    l => l.HasOne<Exchange>().WithMany()
                        .HasForeignKey("ExchangeId")
                        .HasConstraintName("sessionchat_exchange_id_fkey"),
                    j =>
                    {
                        j.HasKey("ExchangeId", "ChatId").HasName("sessionchat_pkey");
                        j.ToTable("sessionchat");
                        j.IndexerProperty<long>("ExchangeId").HasColumnName("exchange_id");
                        j.IndexerProperty<long>("ChatId").HasColumnName("chat_id");
                    });
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("message_pkey");

            entity.ToTable("message");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ChatId).HasColumnName("chat_id");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message1).HasColumnName("message");
            entity.Property(e => e.MessageType).HasColumnName("message_type");
            entity.Property(e => e.SentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("sent_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Chat).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ChatId)
                .HasConstraintName("message_chat_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Messages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("message_user_id_fkey");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.HasKey(e => e.ReactionId).HasName("reaction_pkey");

            entity.ToTable("reaction");

            entity.Property(e => e.ReactionId).HasColumnName("reaction_id");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_date");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.ReactionType).HasColumnName("reaction_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Message).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.MessageId)
                .HasConstraintName("reaction_message_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("reaction_user_id_fkey");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("skill_pkey");

            entity.ToTable("skill");

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.SkillName).HasColumnName("skill_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("User_pkey");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "User_email_key").IsUnique();

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.PwdHash).HasColumnName("pwd_hash");
            entity.Property(e => e.LoginAttempts).HasColumnName("login_attempts").HasDefaultValue(0);
            entity.Property(e => e.SubscriptionStatus)
                .HasDefaultValue(false)
                .HasColumnName("subscription_status");
            entity.Property(e => e.Ulid).HasColumnName("ulid");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        modelBuilder.Entity<Userskill>(entity =>
        {
            entity.HasKey(e => e.UserSkillId).HasName("userskill_pkey");

            entity.ToTable("userskill");

            entity.Property(e => e.UserSkillId).HasColumnName("user_skill_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.SkillType).HasColumnName("skill_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Skill).WithMany(p => p.Userskills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("userskill_skill_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Userskills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("userskill_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
