package com.techalquimia.logistics.repository;

import com.techalquimia.logistics.entity.EvidenceTypeCatalog;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.Optional;
import java.util.UUID;

public interface EvidenceTypeCatalogRepository extends JpaRepository<EvidenceTypeCatalog, UUID> {

    Optional<EvidenceTypeCatalog> findByCode(String code);

    boolean existsByCode(String code);
}
