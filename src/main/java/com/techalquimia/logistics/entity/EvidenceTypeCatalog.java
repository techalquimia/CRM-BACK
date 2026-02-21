package com.techalquimia.logistics.entity;

import jakarta.persistence.*;
import lombok.*;

import java.util.UUID;

/**
 * Catálogo de tipos de evidencia en base de datos.
 */
@Entity
@Table(name = "evidence_type_catalog")
@Getter
@Setter
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class EvidenceTypeCatalog {

    @Id
    @GeneratedValue(strategy = GenerationType.UUID)
    private UUID id;

    @Column(nullable = false, unique = true, length = 50)
    private String code;

    @Column(nullable = false, length = 100)
    private String name;

    @Column(length = 255)
    private String description;

    @Column(name = "requires_ocr", nullable = false)
    @Builder.Default
    private Boolean requiresOcr = false;
}
