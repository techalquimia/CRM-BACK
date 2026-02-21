package com.techalquimia.logistics.entity;

import jakarta.persistence.*;
import lombok.*;

import java.time.Instant;
import java.util.UUID;

@Entity
@Table(name = "evidences")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class Evidence {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    /** URL de la imagen en GCP Cloud Storage o almacenamiento local */
    @Column(name = "image_url", nullable = false, length = 1024)
    private String imageUrl;

    @Column(name = "date_time", nullable = false)
    private Instant dateTime;

    private Double latitude;
    private Double longitude;

    @ManyToOne(fetch = FetchType.EAGER)
    @JoinColumn(name = "evidence_type_id", nullable = false)
    private EvidenceTypeCatalog evidenceTypeCatalog;

    @Column(name = "is_synced", nullable = false)
    @Builder.Default
    private Boolean isSynced = false;

    @Column(name = "created_at", nullable = false, updatable = false)
    private Instant createdAt;

    @Column(name = "user_id", nullable = false)
    private UUID userId;

    @OneToOne(mappedBy = "evidence", cascade = CascadeType.ALL, orphanRemoval = true, fetch = FetchType.LAZY)
    private TicketWeight ticketWeight;

    @PrePersist
    protected void onCreate() {
        createdAt = Instant.now();
    }
}
