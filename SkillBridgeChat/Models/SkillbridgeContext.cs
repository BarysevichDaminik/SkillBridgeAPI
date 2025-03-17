using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SkillBridgeChat.Models;

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

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Userskill> Userskills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=skillbridge;Username=postgres;Password=9435");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.ChatId).HasName("chat_pkey");

            entity.ToTable("chat");

            entity.HasIndex(e => e.ExchangeId, "IX_chat_exchange_id");

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

            entity.HasIndex(e => e.UserId1, "IX_exchange_user_id_1");

            entity.HasIndex(e => e.UserId2, "IX_exchange_user_id_2");

            entity.Property(e => e.ExchangeId).HasColumnName("exchange_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UserId1).HasColumnName("user_id_1");
            entity.Property(e => e.UserId2).HasColumnName("user_id_2");

            entity.HasOne(d => d.UserId1Navigation).WithMany(p => p.ExchangeUserId1Navigations)
                .HasForeignKey(d => d.UserId1)
                .HasConstraintName("exchange_user_id_1_fkey");

            entity.HasOne(d => d.UserId2Navigation).WithMany(p => p.ExchangeUserId2Navigations)
                .HasForeignKey(d => d.UserId2)
                .HasConstraintName("exchange_user_id_2_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("message_pkey");

            entity.ToTable("message");

            entity.HasIndex(e => e.ChatId, "IX_message_chat_id");

            entity.HasIndex(e => e.UserId, "IX_message_user_id");

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

            entity.HasIndex(e => e.MessageId, "IX_reaction_message_id");

            entity.HasIndex(e => e.UserId, "IX_reaction_user_id");

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

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("token_id");

            entity.ToTable("refresh_token");

            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
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

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AvatarNumber)
                .HasDefaultValue((short)0)
                .HasColumnName("avatar_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.LoginAttempts)
                .HasDefaultValue((short)0)
                .HasColumnName("login_attempts");
            entity.Property(e => e.NextAttemptAt).HasColumnName("next_attempt_at");
            entity.Property(e => e.PwdHash).HasColumnName("pwd_hash");
            entity.Property(e => e.Rating)
                .HasDefaultValue((short)0)
                .HasColumnName("rating");
            entity.Property(e => e.Signalrconnectionid).HasColumnName("signalrconnectionid");
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

            entity.HasIndex(e => e.SkillId, "IX_userskill_skill_id");

            entity.HasIndex(e => e.UserId, "IX_userskill_user_id");

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
